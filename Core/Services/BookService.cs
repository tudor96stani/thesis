using Core.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Core.Utils;
using Core.DTO;
using NLog;
namespace Core.Services
{
    public class BookService
    {
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public List<BookDTO> SearchForBook(string Query)
        {
            
            using (var context = new ApplicationDbContext())
            {
                List<BookDTO> result = context.Books.Include(x => x.Authors)
                    .Where(x => x.Title.Contains(Query) || x.Authors.Any(y => y.FullName.Contains(Query)))
                    .ToList()
                    .Select(x => x.ToDTO()).ToList();
                _logger.Debug($"BookService/SearchForBook Query={Query}, found {result.Count} results.");
                return result;
            }
        }


        public void AddBookToLibrary(string UserId,Guid BookId,bool Public = true)
        {
            using (var context = new ApplicationDbContext())
            {
                UsersBooks UserBookEntry = new UsersBooks()
                {
                    BookId = BookId,
                    UserId = UserId,
                    Public = Public,
                    Borrowed = false
                };
                ApplicationUser user = context.Users.Include(x=>x.Books).FirstOrDefault(x => x.Id == UserId);
                if (user == null)
                {
                    _logger.Warn($"BookService/AddBookToLibrary Could not find user with Id={UserId}");
                    throw new Exception("User with given ID does not exist");
                }

                Book book = context.Books.Include(x => x.Owners).FirstOrDefault(x => x.Id == BookId);
                if (book == null)
                {
                    _logger.Warn($"BookService/AddBookToLibrary Could not find book with Id={BookId}");
                    throw new Exception("Book with given ID does not exist");
                }

                
                var addResult = context.UsersBooks.Add(UserBookEntry);
                if (addResult == null)
                {
                    _logger.Warn($"BookService/AddBookToLibrary Could not add book with Id={BookId} to User with Id={UserId} library.");
                    throw new Exception("Could not add book to library.");
                }

                user.Books.Add(UserBookEntry);
                book.Owners.Add(UserBookEntry);
                _logger.Debug($"BookService/AddBookToLibrary Book (Id={BookId}) added to User (Id={UserId}) library");

                Activity activity = new Activity()
                {
                    Id = Guid.NewGuid(),
                    OwnerId = UserId,
                    TimeStampUTC = DateTime.Now.ToUniversalTime(),
                    Type = ActivityType.AddedBook,
                    BookId = BookId
                };
                context.Activities.Add(activity);
                context.SaveChanges();
            }
        }

        public void AddNewBookToLibrary(string UserId,BookDTO book,List<string> authorNames)
        {
            using (var context = new ApplicationDbContext())
            {
                List<Author> authors = context.Authors.Where(x => authorNames.Contains(x.FullName)).ToList();
                if(authorNames.Count!=authors.Count)
                {
                    IEnumerable<string> newAuthors = authorNames.Except(authors.Select(x => x.FullName));
                    _logger.Debug($"BookService/AddNewBookToLibrary {newAuthors.Count()} new authors are being added.");
                    List<Author> newAuthorsObjects = new List<Author>();
                    foreach(var newAuth in newAuthors)
                    {
                        newAuthorsObjects.Add(new Author()
                        {
                            Id=Guid.NewGuid(),
                            FullName = newAuth
                        });
                    }
                    context.Authors.AddRange(newAuthorsObjects);
                    authors.AddRange(newAuthorsObjects);
                }

                Book bookObj = new Book()
                {
                    Id = Guid.NewGuid(),
                    Title = book.Title,
                    Year = book.Year,
                    Publisher = book.Publisher
                };
                bookObj.Authors = authors;
                context.Books.Add(bookObj);
                foreach(var author in authors)
                {
                    author.Books.Add(bookObj);
                }
                context.SaveChanges();
                AddBookToLibrary(UserId, bookObj.Id);
            }
        }

        public List<BookDTO> GetLibraryFor(string UserId)
        {
            using (var context = new ApplicationDbContext())
            {
                List<Guid> userBookIds = context.Users.Include(x=>x.Books).FirstOrDefault(x => x.Id == UserId)
                    .Books.Select(x=>x.BookId).ToList();
                _logger.Debug($"BookService/GetLibraryFor Found {userBookIds.Count()} for user with Id={UserId}");
                List<BookDTO> actualBooks = context.Books.Where(x => userBookIds.Contains(x.Id))
                    .ToList().Select(x => x.ToDTO()).ToList();
                if (userBookIds.Count() != actualBooks.Count())
                    _logger.Warn($"BookService/GetLibraryFor Found {userBookIds.Count()} book IDs but {actualBooks.Count()} actual books retrieved from DB.");
                return actualBooks;
            }
        }
    }
}

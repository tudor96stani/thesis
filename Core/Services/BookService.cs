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
                List<BookDTO> result = context.Books.Include(x => x.Authors).Include(x=>x.BookImage)
                    .Where(x => x.Title.Contains(Query) || x.Authors.Any(y => y.FullName.Contains(Query)))
                    .ToList()
                    .Select(x => x.ToDTO()).ToList();
                _logger.Debug($"BookService/SearchForBook Query={Query}, found {result.Count} results.");
                return result;
            }
        }

        public bool CheckIfBookInLibrary(string userId,string bookId)
        {
            using(var context = new ApplicationDbContext())
            {
                var result = context.UsersBooks.FirstOrDefault(x => x.UserId == userId && x.BookId == new Guid(bookId));
                return result != null;
             
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
                    Borrowed = false,
                    Lent = false
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
                BookImage img = new BookImage()
                {
                    BookImageId = bookObj.Id,
                    Content = book.Cover
                    //Book = bookObj
                };
                context.Images.Add(img);
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
                List<BookDTO> actualBooks = context.Books.Include(x => x.BookImage).Include(x => x.Authors).Where(x => userBookIds.Contains(x.Id))
                    .ToList().Select(ToDTOConverter.ToDTO).ToList();
                List<UsersBooks> usersBooks = context.UsersBooks.Include(x=>x.LentTo).Include(x=>x.BorrowedFrom).Where(x => userBookIds.Contains(x.BookId)).ToList();

                if (userBookIds.Count() != actualBooks.Count())
                    _logger.Warn($"BookService/GetLibraryFor Found {userBookIds.Count()} book IDs but {actualBooks.Count()} actual books retrieved from DB.");
                
                foreach (var book in actualBooks)
                {
                    var correspondingUserBook = usersBooks.FirstOrDefault(x => x.BookId == book.Id && x.UserId == UserId);
                    if (correspondingUserBook != null)
                    {
                        book.Lent = correspondingUserBook.Lent;
                        book.LentTo = correspondingUserBook.LentTo == null ? correspondingUserBook.LentTo.ToDTO() : null;
                        book.Borrowed = correspondingUserBook.Borrowed;
                        book.BorrowedFrom = correspondingUserBook.BorrowedFrom == null ? correspondingUserBook.BorrowedFrom.ToDTO() : null;
                    }
                    else
                    {
                        book.Lent = false;
                        book.Borrowed = false;
                    }
                }
                return actualBooks;
            }
        }

        public List<UserDTO> GetUsersWhoOwnBook(Guid bookId,string loggedInUserId)
        {
            using (var context = new ApplicationDbContext())
            {
                List<string> userIds = context.UsersBooks.Where(x => x.BookId == bookId && x.UserId != loggedInUserId)
                    .Select(x => x.UserId).ToList();

                List<UserDTO> users = context.Users.Where(x => userIds.Contains(x.Id))
                        .ToList().Select(ToDTOConverter.ToDTO).ToList();
                return users;
            }
        }

        public void RequestBorrowBook(string borrower,string lender,Guid bookId)
        {
            if (borrower == null || lender==null || bookId == null || borrower == lender)
                throw new Exception("Cannot make request");

            using (var context = new ApplicationDbContext())
            {
                BorrowRequest request = new BorrowRequest()
                {
                    BookId = bookId,
                    BorrowerId = borrower,
                    LenderId = lender
                };

                context.BorrowRequests.Add(request);
                context.SaveChanges();
            }
        }

        public List<BorrowRequestDTO> GetBorrowRequests(string userId)
        {
            using (var context = new ApplicationDbContext())
            {
                List<BorrowRequest> requests = context.BorrowRequests.Where(x => x.LenderId == userId).ToList();
                List<Guid> bookIds = requests.Select(x => x.BookId).ToList();
                List<BookDTO> books = context.Books.Where(x => bookIds.Contains(x.Id)).ToList().Select(ToDTOConverter.ToDTO).ToList();
                List<string> userIds = requests.Select(x => x.BorrowerId).ToList();
                List<UserDTO> users = context.Users.Where(x => userIds.Contains(x.Id)).ToList().Select(ToDTOConverter.ToDTO).ToList();
                List<BorrowRequestDTO> result = requests.Select(x => x.ToDTO(books.FirstOrDefault(y => y.Id == x.BookId), users.FirstOrDefault(z => z.Id == x.BorrowerId))).ToList();
                return result;
            }
        }

        public void AcceptBorrowBookRequest(string requester,string borrowFromId,Guid bookId)
        {
            if (requester == borrowFromId)
                throw new Exception("Cannot borrow from self.");
            using (var context = new ApplicationDbContext())
            {
                UsersBooks userBook = context.UsersBooks.FirstOrDefault(x => x.BookId == bookId && x.UserId == borrowFromId);
                if (userBook == null)
                {
                    //LOG
                    throw new Exception("Cannot find UserBook");
                }


                var book = context.Books.FirstOrDefault(x => x.Id == bookId);

                ApplicationUser borrower = context.Users.FirstOrDefault(x => x.Id == requester);
                if (borrower == null)
                {
                    throw new Exception("Borrower does not exist");
                }

                userBook.Lent = true;
                userBook.LentToId = borrower.Id;
                userBook.LentTo = borrower;


                ApplicationUser borrowFrom = context.Users.FirstOrDefault(x => x.Id == borrowFromId);
                if (borrowFrom == null)
                {
                    throw new Exception("BorrowFrom does not exist");
                }
                var newUserBook = new UsersBooks()
                {
                    BookId = bookId,
                    UserId = borrower.Id,
                    Public = true,
                    Borrowed = true,
                    BorrowedFrom = borrowFrom,
                    BorrowedFromId = borrowFromId,
                    Lent = false,
                    Book = book
                };
                context.UsersBooks.Add(newUserBook);
              
                context.SaveChanges();
            }
        }
    }
}

using Core.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Core.Utils;
using Core.DTO;
namespace Core.Services
{
    public class BookService
    {
        public List<BookDTO> SearchForBook(string Query)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Books.Include(x => x.Authors)
                    .Where(x => x.Title.Contains(Query) || x.Authors.Any(y => y.FullName.Contains(Query)))
                    .ToList()
                    .Select(x => x.ToDTO()).ToList();
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
                    throw new Exception("User with given ID does not exist");
                

                Book book = context.Books.Include(x => x.Owners).FirstOrDefault(x => x.Id == BookId);
                if (book == null)
                    throw new Exception("Book with given ID does not exist");

                
                var addResult = context.UsersBooks.Add(UserBookEntry);
                if (addResult == null)
                    throw new Exception("Could not add book to library.");

                user.Books.Add(UserBookEntry);
                book.Owners.Add(UserBookEntry);
                context.SaveChanges();
            }
        }

        public List<BookDTO> GetLibraryFor(string UserId)
        {
            using (var context = new ApplicationDbContext())
            {
                List<Guid> userBookIds = context.Users.Include(x=>x.Books).FirstOrDefault(x => x.Id == UserId)
                    .Books.Select(x=>x.BookId).ToList();
                List<BookDTO> actualBooks = context.Books.Where(x => userBookIds.Contains(x.Id))
                    .ToList().Select(x => x.ToDTO()).ToList();
                return actualBooks;
            }
        }
    }
}

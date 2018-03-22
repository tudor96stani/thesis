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
    }
}

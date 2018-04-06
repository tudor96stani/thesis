using Core.DAL;
using Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils
{
    public static class ToDTOConverter
    {
        public static BookDTO ToDTO(this Book model)
        {
            return model == null ? null : new BookDTO()
            {
                Id = model.Id,
                Title = model.Title,
                Year = model.Year,
                Publisher = model.Publisher,
                Authors = model.Authors.Select(x => x.ToDTO()).ToList(),
                Cover=model.BookImage!=null ? model.BookImage.Content : null
            };
        }

        public static AuthorDTO ToDTO(this Author model)
        {
            return model == null ? null : new AuthorDTO()
            {
                Id=model.Id,
                FirstName=model.FirstName,
                LastName=model.LastName,
                Fullname = model.FullName
            };
        }

        public static UserDTO ToDTO(this ApplicationUser model)
        {
            return model == null ? null : new UserDTO()
            {
                Id = model.Id,
                Name = model.UserName,
                Password=model.PasswordHash
            };
        }

        
    }
}

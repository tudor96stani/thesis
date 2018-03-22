using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Core.DAL
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public int Year { get; set; }
        public string Publisher { get; set; }

        public virtual ICollection<Author> Authors { get; set; }
        public virtual ICollection<UsersBooks> Owners { get; set; }

        public Book()
        {
            this.Authors = new HashSet<Author>();
            this.Owners = new HashSet<UsersBooks>();
        }
    }
}
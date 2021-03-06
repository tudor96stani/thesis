﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Core.DAL
{
    public class Author
    {
        [Key]
        public Guid Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
       
        public virtual ICollection<Book> Books { get; set; }
        public string FullName { get; set; }

        public Author()
        {
            this.Books = new HashSet<Book>();
        }
    }
}
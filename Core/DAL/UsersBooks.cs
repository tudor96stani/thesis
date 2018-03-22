using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Core.DAL
{
    public class UsersBooks
    {
        [Required]
        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }

        [Required]
	    public virtual Book Book { get; set; }
        public Guid BookId { get; set; }

	    public bool Public { get; set; }
	    public bool Borrowed { get; set; }

	    public virtual ApplicationUser Borrower { get; set; }
        public string BorrowerId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.ViewModels
{
    public class BorrowRequestViewModel
    {
        public string From { get; set; }
        public Guid BookId { get; set; }
    }
}
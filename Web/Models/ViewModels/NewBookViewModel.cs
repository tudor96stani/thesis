using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.ViewModels
{
    public class NewBookViewModel
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public int Year { get; set; }
        public string Publisher { get; set; }
    }
}
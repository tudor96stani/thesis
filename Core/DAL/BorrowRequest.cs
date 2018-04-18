using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DAL
{
    public class BorrowRequest
    {
        public Guid BookId { get; set; }
        public string BorrowerId { get; set; }
        public string LenderId { get; set; }
    }
}

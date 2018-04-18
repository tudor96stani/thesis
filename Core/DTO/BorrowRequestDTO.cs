using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class BorrowRequestDTO
    {
        public BookDTO Book { get; set; }
        public UserDTO Borrower { get; set; }
    }
}

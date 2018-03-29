using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class FriendshipDTO
    {
        public UserDTO Me { get; set; }
        public UserDTO Other { get; set; }
    }
}

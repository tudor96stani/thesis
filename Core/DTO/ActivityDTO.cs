using Core.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class ActivityDTO
    {
        public Guid Id { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public ActivityType Type { get; set; }
        public BookDTO Book { get; set; }
        public UserDTO Owner { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DAL
{
    public enum ActivityType
    {
        AddedBook,
        BorrowedBook
    }
    public class Activity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string OwnerId { get; set; }
        [Required]
        public DateTime TimeStampUTC { get; set; }
        [Required]
        public ActivityType Type { get; set; }
        [Required]
        public Guid BookId { get; set; }
    }
}

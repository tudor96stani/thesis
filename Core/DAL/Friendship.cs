using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Core.DAL
{
    public enum RelationshipStatus
    {
        Accepted,Requested
    }
    public class Friendship
    {
        public virtual ApplicationUser User1 { get; set; }
        [Required]
        public string User1Id { get; set; }

        public virtual ApplicationUser User2 { get; set; }
        [Required]
        public string User2Id { get; set; }

        public RelationshipStatus Status { get; set; } 
    }
}
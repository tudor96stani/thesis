using Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.ViewModels
{
    public class FriendRequestViewModel
    {
        public UserDTO User { get; set; }
        public int CommonFriendsCount { get; set; }
    }
}
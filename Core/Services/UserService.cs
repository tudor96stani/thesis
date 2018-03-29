using Core.DAL;
using Core.DTO;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class UserService
    {
        public void AddFriend(string Requester, string Requested)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingRelationship = context.Friendships.FirstOrDefault(x => x.User1Id == Requested && x.User2Id == Requester);
                if(existingRelationship!=null)
                {
                    //LOG
                    throw new Exception("Relationship already exists!");
                }
                var friendship = new Friendship()
                {
                    User1Id = Requester,
                    User2Id = Requested,
                    Status = RelationshipStatus.Requested
                };
                var addToContextResult = context.Friendships.Add(friendship);
                if (addToContextResult == null)
                {
                    //LOG
                    throw new Exception("Could not establish relationship");
                }
                var user1 = context.Users.FirstOrDefault(x => x.Id == Requester);
                user1.MyFriends.Add(friendship);

                var user2 = context.Users.FirstOrDefault(x => x.Id == Requested);
                user2.FriendsWithMe.Add(friendship);

                context.SaveChanges();
            }
        }

        public void AcceptRequest(string Me, string Requester)
        {
            using (var context = new ApplicationDbContext())
            {
                var friendship = context.Friendships.FirstOrDefault(x => x.User2Id == Me && x.User1Id == Requester);
                if (friendship == null)
                {
                    //LOG
                    throw new Exception("Could not find friendship");
                }

                if (friendship.Status != RelationshipStatus.Requested)
                {
                    //LOG
                    throw new Exception("Already friends");
                }
                friendship.Status = RelationshipStatus.Accepted;
                context.SaveChanges();
            }
        }

        public List<UserDTO> GetPendingRequests(string UserId)
        {
            using (var context = new ApplicationDbContext())
            {
                var userIds = context.Friendships
                    .Where(x => x.User2Id == UserId && x.Status == RelationshipStatus.Requested)
                    .ToList()
                    .Select(x => x.User1Id)
                    .ToList();

                if (userIds==null)
                {
                    //LOG
                    throw new Exception("No requests found");
                }
                var users = context.Users.Where(x => userIds.Contains(x.Id)).ToList();

                return users.Select(x => x.ToDTO()).ToList();
            }
        }
    }
}

﻿using Core.DAL;
using Core.DTO;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using System.Data.Entity;
namespace Core.Services
{
    public class UserService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public void AddFriend(string Requester, string Requested)
        {
            if (Requester == Requested)
            {
                _logger.Warn($"UserService/AddFriend Trying to create relationship between user and self");
                throw new Exception("Cannot add self as friend");
            }
            using (var context = new ApplicationDbContext())
            {
                var existingRelationship = context.Friendships.FirstOrDefault(x => x.User1Id == Requested && x.User2Id == Requester);
                if(existingRelationship!=null)
                {
                    _logger.Warn($"UserService/AddFriend User with Id={Requester} sent friend request to Id={Requested} - relationship already exists.");
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
                    _logger.Warn($"UserService/AddFriend User with Id={Requester} sent friend request to Id={Requested} - couldn't create relationship.");
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
                    _logger.Warn($"UserService/AcceptRequest Relationship between Me={Me} and Requester={Requester} does not exist");
                    throw new Exception("Could not find friendship");
                }

                if (friendship.Status != RelationshipStatus.Requested)
                {
                    _logger.Warn($"UserService/AcceptRequest Relationship status between Me={Me} and Requester={Requester} already Accepted");
                    throw new Exception("Already friends");
                }
                friendship.Status = RelationshipStatus.Accepted;
                context.SaveChanges();
            }
        }

        public void RejectRequest(string Me, string Requester)
        {
            using(var context = new ApplicationDbContext())
            {
                var friendship = context.Friendships.FirstOrDefault(x => x.User2Id == Me && x.User1Id == Requester);
                if (friendship == null)
                {
                    _logger.Warn($"UserService/AcceptRequest Relationship between Me={Me} and Requester={Requester} does not exist");
                    throw new Exception("Could not find friendship");
                }

                if (friendship.Status != RelationshipStatus.Requested)
                {
                    _logger.Warn($"UserService/AcceptRequest Relationship status between Me={Me} and Requester={Requester} already Accepted");
                    throw new Exception("Already friends");
                }
                
                var me = context.Users.Include(x => x.FriendsWithMe).FirstOrDefault(x => x.Id == Me);
                me.FriendsWithMe.Remove(friendship);
                var requester = context.Users.Include(x => x.MyFriends).FirstOrDefault(x => x.Id == Requester);
                requester.MyFriends.Remove(friendship);

                context.Friendships.Remove(friendship);
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
                    _logger.Info($"UserService/GetPendingRequests UserId={UserId} No relationships found.");
                    throw new Exception("No requests found");
                }
                var users = context.Users.Where(x => userIds.Contains(x.Id)).ToList();
               // return users.Select(ToDTOConverter.ToDTO).ToList();
                return users.Select(x => x.ToDTO()).ToList();
            }
        }

        public int GetPendingRequestsNumber(string UserId)
        {
            using (var context = new ApplicationDbContext())
            {
                var userIds = context.Friendships
                    .Where(x => x.User2Id == UserId && x.Status == RelationshipStatus.Requested)
                    .ToList()
                    .Select(x => x.User1Id)
                    .ToList();

                if (userIds == null)
                {
                    _logger.Info($"UserService/GetPendingRequests UserId={UserId} No relationships found.");
                    throw new Exception("No requests found");
                }
                return userIds.Count();
            }
        }

        public List<UserDTO> GetFriendsFor(string UserId)
        {
            using (var context = new ApplicationDbContext())
            {
                List<string> friendsIds = context.Friendships
                    .Where(x => x.User1Id == UserId || x.User2Id == UserId)
                    .Where(x=>x.Status == RelationshipStatus.Accepted)
                    .Select(x => x.User1Id == UserId ? x.User2Id : x.User1Id)
                    .ToList();

                _logger.Debug($"UserService/GetFriendsFor Found {friendsIds.Count()} for user with Id={UserId}");

                List<UserDTO> friends = context.Users
                                    .Where(x => friendsIds.Contains(x.Id))
                                    .ToList()
                                    .Select(x => x.ToDTO()).ToList();
                return friends;
                
            }
        }

        public List<ActivityDTO> GetNewsFeed(string UserId,int page)
        {
            using (var context = new ApplicationDbContext())
            {
                List<string> friendsIds = context.Friendships
                    .Where(x => x.User1Id == UserId || x.User2Id == UserId)
                    .Select(x => x.User1Id == UserId ? x.User2Id : x.User1Id)
                    .ToList();
                List<Activity> activities = context.Activities
                        .Where(x => friendsIds.Contains(x.OwnerId))
                        .OrderByDescending(x => x.TimeStampUTC)
                        .Skip((page - 1) * 10)
                        .Take(10)
                        .ToList();
                List<Guid> BooksIds = activities.Select(x => x.BookId).ToList();
                List<BookDTO> Books = context.Books.Where(x => BooksIds.Contains(x.Id))
                                        .ToList().Select(x => x.ToDTO()).ToList();

                List<string> UserIds = activities.Select(x => x.OwnerId).ToList();
                List<UserDTO> Users = context.Users.Where(x => UserIds.Contains(x.Id))
                                        .ToList().Select(x => x.ToDTO()).ToList();

                List<ActivityDTO> activitiesDTOs = activities
                        .Select(x => x.ToDTO(Books.FirstOrDefault(y => y.Id == x.BookId),
                        Users.FirstOrDefault(z => z.Id == x.OwnerId))).ToList();
                return activitiesDTOs;
            }
        }

       public List<UserDTO> FindUsers(string query,string loggedInUserId)
        {
            using (var context = new ApplicationDbContext())
            {
                var users = context.Users.Where(x => x.UserName.Contains(query) && x.Id != loggedInUserId)
                        .ToList();
                return users.Select(x => x.ToDTO()).ToList();
            }
        }

        public int GetNumberOfCommonFriends(string me, string other)
        {
            if (me==null || other == null || me == other)
            {
                throw new Exception("Bad request");
            }
            using(var context = new ApplicationDbContext())
            {
                var myFriendsIds = context.Friendships.Where(x => x.User1Id == me).Select(x => x.User2Id).ToList();
                myFriendsIds.Union(context.Friendships.Where(x => x.User2Id == me).Select(x => x.User1Id).ToList());

                var otherFriendsIds = context.Friendships.Where(x => x.User1Id == other).Select(x => x.User2Id).ToList();
                otherFriendsIds.Union(context.Friendships.Where(x => x.User2Id == other).Select(x => x.User1Id).ToList());

                int result = myFriendsIds.Intersect(otherFriendsIds).Count();
                return result;
            }
        }
    }
}

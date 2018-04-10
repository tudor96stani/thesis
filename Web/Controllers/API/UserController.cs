using Core.DTO;
using Core.Services;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Web.Models;
using Web.Utils;

namespace Web.Controllers.API
{
    [RoutePrefix("api/v1/user")]
    [Authorize]
    public class UserController : ApiController
    {

        private AuthRepository _repo = null;
        private readonly UserService _userService = new UserService();
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public UserController()
        {
            _repo = new AuthRepository();
        }

        [Authorize]
        [Route("verify")]
        [HttpGet]
        public IHttpActionResult Check()
        {
            return Ok();
        }

        // POST api/v1/user/Register
        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        public async Task<IHttpActionResult> Register(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.Warn("UserController/Register Model state not valid.");
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(model.Username,model.Password);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        [HttpPost]
        [Route("requests/add")]
        public HttpResponseMessage AddFriend(string userid)
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                _userService.AddFriend(loggedInUserId, userid);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                _logger.Warn($"UserController/AddFriend Exception message={e.Message}");
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }

        [HttpGet]
        [Route("requests")]
        public HttpResponseMessage GetRequests()
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                var result = _userService.GetPendingRequests(loggedInUserId);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
                return response;
            }
            catch(Exception e)
            {
                _logger.Warn($"UserController/GetRequests Exception message={e.Message}");
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }

        [HttpPost]
        [Route("requests/accept")]
        public HttpResponseMessage AcceptRequest(string userId)
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                _userService.AcceptRequest(loggedInUserId, userId);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                _logger.Warn($"UserController/AcceptRequest Exception message={e.Message}");
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }

        [HttpGet]
        [Route("friends")]
        public HttpResponseMessage GetFriendsFor()
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                _logger.Debug($"UserController/GetFriendsFor Searching for friends of user with id={loggedInUserId}");
                List<UserDTO> friends = _userService.GetFriendsFor(loggedInUserId);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, friends);
                return response;
            }
            catch(Exception e)
            {
                _logger.Warn($"UserController/GetFriendsFor Exception message={e.Message}");
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }

        [HttpGet]
        [Route("newsfeed")]
        public HttpResponseMessage GetNewsFeed(int page = 1)
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                var result = _userService.GetNewsFeed(loggedInUserId, page);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
                return response;
            }catch(Exception e)
            {
                _logger.Warn($"UserController/GetNewsFeed Exception message={e.Message}");
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}

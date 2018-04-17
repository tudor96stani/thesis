using Core.DTO;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using NLog;
namespace Web.Controllers.API
{
    //[Authorize]
    [RoutePrefix("api/v1/books")]
    public class BooksController : ApiController
    {
        private readonly BookService _bookService = new BookService();
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("search")]
        public List<BookDTO> SearchForBook(string query)
        {
            var books = _bookService.SearchForBook(query.Replace('%',' '));
            return books;
        }

        [HttpGet]
        [Route("check")]
        public HttpResponseMessage CheckIfBookInLibrary(string bookId)
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            var result = _bookService.CheckIfBookInLibrary(loggedInUserId, bookId);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, new { inLibrary = result});
            return response;
        }


        [HttpGet]
        [Route("{userid}")]
        public List<BookDTO> GetLibraryFor(string userid)
        {
            try
            {
                return _bookService.GetLibraryFor(userid);
            }catch(Exception e)
            {
                _logger.Error($"BooksController/GetLibraryFor Message={e.Message}");
                throw new HttpRequestException("Could not load books for the user");
            }
        }


        [HttpPost]
        [Route("add")]
        public HttpResponseMessage AddBookToLibrary([FromBody]SingleStringParam bookId)
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                _bookService.AddBookToLibrary(loggedInUserId, new Guid(bookId.Param));
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                _logger.Error($"BooksController/AddBookToLibrary Message={e.Message}");
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError,e.Message);
                return response;
            }
        }

        [HttpPost]
        [Route("addnew")]
        public HttpResponseMessage AddNewBookToLibrary(NewBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cnt = model.Authors != null ? model.Authors.Count() : -1;
                var error = String.Join("\t", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToArray());
                _logger.Debug($"BookController/AddNewBookToLibrary Model errors = {error}");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(model.CoverUrl);
                _bookService.AddNewBookToLibrary(loggedInUserId, new BookDTO()
                {
                    Title = model.Title,
                    Year = model.Year,
                    Publisher = model.Publisher,
                    Cover = imageBytes
                }, model.Authors);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }catch(Exception e)
            {
                _logger.Error($"BooksController/AddNewBookToLibrary Message={e.Message}");
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }

        [HttpGet]
        [Route("owners")]
        public HttpResponseMessage GetOwnersOfBook(Guid bookId)
        {
            string loggedInUserId = RequestContext.Principal.Identity.GetUserId();
            try
            {
                List<UserDTO> users = _bookService.GetUsersWhoOwnBook(bookId,loggedInUserId);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, users);
                return response;
            }
            catch (Exception e)
            {
                _logger.Error($"BooksController/GetOwnersOfBook Message={e.Message}");
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }
    }
}

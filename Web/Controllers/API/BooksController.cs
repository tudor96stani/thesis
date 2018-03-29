using Core.DTO;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Web.Models.ViewModels;
using NLog;
namespace Web.Controllers.API
{
    [RoutePrefix("api/v1/books")]
    public class BooksController : ApiController
    {
        private readonly BookService _bookService = new BookService();
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("search")]
        public List<BookDTO> SearchForBook(string query)
        {
            var books = _bookService.SearchForBook(query);
            return books;
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
        [Route("{userid}")]
        public HttpResponseMessage AddBookToLibrary([FromUri] string userid,[FromBody]string bookId)
        {
            try
            {
                _bookService.AddBookToLibrary(userid, new Guid(bookId));
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
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            try
            {
                _bookService.AddNewBookToLibrary(model.UserId, new BookDTO()
                {
                    Title = model.Title,
                    Year = model.Year,
                    Publisher = model.Publisher
                }, model.Authors);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }catch(Exception e)
            {
                _logger.Error($"BooksController/AddNewBookToLibrary Message={e.Message}");
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                return response;
            }
        }
    }
}

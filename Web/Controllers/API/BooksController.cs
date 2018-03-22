using Core.DTO;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Web.Controllers.API
{
    [RoutePrefix("api/v1/books")]
    public class BooksController : ApiController
    {
        private readonly BookService _bookService = new BookService();

        [HttpGet]
        [Route("search")]
        public List<BookDTO> SearchForBook(string query)
        {
            var books = _bookService.SearchForBook(query);
            return books;
        }
    }
}

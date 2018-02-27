using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GenericMVC.Models;

namespace GenericMVC.Controllers
{
    public class AuthorBooksController : GenericController<AuthorBook>
    {
        public AuthorBooksController(LibraryContext context) : base(context)
        {
        }
    }
}
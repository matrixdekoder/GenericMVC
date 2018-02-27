using GenericMVC.Controllers;
using GenericMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMVC.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(LibraryContext _context) : base(_context)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GenericMVC.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GenericMVC.Controllers
{
    public class BaseController : Controller
    {
        protected LibraryContext _context;

        public BaseController(LibraryContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var assembly = typeof(BaseController).Assembly;
            var types = assembly.GetTypes();
            var types1 = types.Where(i => i.BaseType.Name.Contains("GenericController"));
            ViewBag.Controllers = types1;
        }
    }
}
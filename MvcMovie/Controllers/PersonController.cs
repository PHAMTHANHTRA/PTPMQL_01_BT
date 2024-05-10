using FirstMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace MvcMovie.Controllers
{
    public class PersonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]

        public IActionResult index(Person ps)
        {
            string strOutput = "Xin chao" + ps.PersonID + "-" +ps.FullName + "-" +ps.Address;
            ViewBag.infoPerson = strOutput;
            return View();
        }
    }
}
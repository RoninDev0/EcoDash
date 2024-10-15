using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;


namespace EcoDash.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}

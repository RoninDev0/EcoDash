using EcoDash.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EcoDash.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            var isError = HttpContext.Request.Query["error"].ToString();
            if (string.IsNullOrEmpty(isError) || isError != "true")
            {
                return RedirectToAction("Index", "Home");
            }

            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var viewmodel = new ErrorModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = exceptionHandlerPathFeature?.Error.Message
            };

            return View(viewmodel);
        }
    }
}

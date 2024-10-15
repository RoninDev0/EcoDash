using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EcoDash.Services;
using Newtonsoft.Json.Linq;

namespace EcoDash.Controllers
{
    public class RouteController : Controller
    {
        private readonly RouteService _routeService;

        public RouteController(RouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetRoutes(string startLocation, string destination, string mode)
        {
            if (string.IsNullOrEmpty(startLocation) || string.IsNullOrEmpty(destination))
            {
                ModelState.AddModelError("", "Both start and destination locations are required.");
                return View("Index");
            }

            // Fetch eco-friendly routes from the RouteService based on the selected mode
            var routesData = await _routeService.GetEcoFriendlyRoutes(startLocation, destination, mode);

            // Handle the case where no routes are found or an error occurs
            if (routesData["status"]?.ToString() == "ZERO_RESULTS" || routesData["routes"]?.Count() == 0)
            {
                ViewBag.ErrorMessage = "No routes found. Please check the locations entered.";
                return View("RouteResults");
            }

            // Pass valid route data to the view
            return View("RouteResults", routesData);
        }
    }
}

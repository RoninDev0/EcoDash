using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using EcoDash.Data;
using EcoDash.Models;


namespace EcoDash.Controllers
{
    public class LoginController : Controller
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public LoginController(MongoDbContext dbContext)
        {
            _collection = dbContext.GetCollection<BsonDocument>("Members");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Message"] = "Sucessfully Logged Out!";

            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public IActionResult Verify(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                (var isValid, var GUID) = MongoDbUtils.LoginUser(model, _collection);

                if (isValid)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, GUID),
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        authProperties);

                    return RedirectToAction("Index", "Dashboard");

                }
                else
                {
                    TempData["Message"] = "Invalid Login Credentials!";
                }

            }
            return View("Index", model);
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }
    }
}

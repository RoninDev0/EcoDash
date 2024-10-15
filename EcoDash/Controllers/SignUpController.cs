using EcoDash.Data;
using EcoDash.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;

namespace EcoDash.Controllers
{
    public class SignUpController : Controller
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public SignUpController(MongoDbContext dbContext)
        {
            _collection = dbContext.GetCollection<BsonDocument>("Members");
        }
        [HttpPost]
        public IActionResult Verify(SignUpModel model)
        {
            if (ModelState.IsValid)
            {
                (var isValid, var CustomMessage) = MongoDbUtils.SignupUser(model, _collection);

                if (isValid)
                {
                    TempData["Message"] = "Succesfully Registered!";
                }
                else
                {
                    TempData["Message"] = CustomMessage;
                }

            }
            return View("Index", model);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}

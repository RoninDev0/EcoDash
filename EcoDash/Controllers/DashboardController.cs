using EcoDash.Data;
using EcoDash.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;

namespace EcoDash.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public DashboardController(MongoDbContext dbContext)
        {
            _collection = dbContext.GetCollection<BsonDocument>("Members");
        }
        public IActionResult Index()
        {
            // Retrieve the user's GUID from the claims
            string GUID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(GUID))
            {
                return Unauthorized("User identifier not found.");
            }

            // Fetch the user's data from MongoDB
            var filter = Builders<BsonDocument>.Filter.Eq("GUID", GUID);
            var user = _collection.Find(filter).FirstOrDefault();

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map MongoDB data to DashboardViewModel
            var model = new DashboardModel
            {
                TotalGreenCommutes = user.Contains("totalGreenCommutes") ? user["totalGreenCommutes"].ToInt32() : 0,
                TotalCo2Saved = user.Contains("totalCo2Saved") ? user["totalCo2Saved"].ToDouble() : 0.0,
                TotalCaloriesBurned = user.Contains("totalCaloriesBurned") ? user["totalCaloriesBurned"].ToDouble() : 0.0,
                TotalPoints = user.Contains("totalPoints") ? user["totalPoints"].ToInt32() : 0,
                TopEcoScore = user.Contains("topEcoScore") ? user["topEcoScore"].ToDouble() : 0.0
            };

            // Pass the ViewModel to the view
            return View(model);
        }
    }
}

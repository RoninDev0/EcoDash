using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using EcoDash.Data;
using EcoDash.Models;
using MongoDB.Driver.GridFS;

namespace EcoDash.Controllers
{
    [Authorize]
    public class LeaderboardController : Controller
    {
        private readonly IMongoCollection<BsonDocument> _membersCollection;
        private readonly IMongoCollection<BsonDocument> _routesCollection;
        private readonly GridFSBucket _gridFSBucket;

        public LeaderboardController(MongoDbContext dbContext)
        {
            _membersCollection = dbContext.GetCollection<BsonDocument>("Members");
            _routesCollection = dbContext.GetCollection<BsonDocument>("Routes");
            _gridFSBucket = new GridFSBucket(dbContext.Database);  // Assuming you are using GridFS to store images
        }

        public IActionResult Index()
        {
            // Get the current user's GUID
            string currentUserGuid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserGuid))
            {
                return Unauthorized("User identifier not found.");
            }

            // Fetch the top users by points, including the current user
            var filter = Builders<BsonDocument>.Filter.Exists("totalPoints");
            var sort = Builders<BsonDocument>.Sort.Descending("totalPoints");
            var topUsers = _membersCollection.Find(filter).Sort(sort).Limit(10).ToList(); // Top 10 users

            // Ensure the current user is included in the leaderboard
            var currentUser = _membersCollection.Find(Builders<BsonDocument>.Filter.Eq("GUID", currentUserGuid)).FirstOrDefault();
            if (currentUser != null && !topUsers.Any(u => u["GUID"] == currentUser["GUID"]))
            {
                topUsers.Add(currentUser); // Include current user if not in the top 10
            }

            // For each user, fetch their completed routes and associated images
            var leaderboard = new List<LeaderboardModel>();
            foreach (var user in topUsers)
            {
                var userGuid = user["GUID"].ToString();

                // Fetch completed routes for the user
                var routeFilter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("GUID", userGuid),
                    Builders<BsonDocument>.Filter.Eq("Completed", true)
                );

                var completedRoutes = _routesCollection.Find(routeFilter).ToList();

                // Map the completed routes to RouteInfo objects
                var routeInfos = completedRoutes.Select(route => new RouteInfo
                {
                    RouteID = route["_id"].ToString(),
                    ImageFileId = route.Contains("ImageFileId") ? route["ImageFileId"].ToString() : null,
                    Distance = route["legs"][0]["distance"]["value"].ToDouble() / 1000,  // Assuming distance is stored in meters
                    Co2Saved = route["co2Saved"].ToDouble()
                }).ToList();

                // Create the leaderboard entry for the user
                leaderboard.Add(new LeaderboardModel
                {
                    Username = user["Username"].ToString(),
                    TotalPoints = user["totalPoints"].ToInt32(),
                    GUID = user["GUID"].ToString(),
                    FinishedRoutes = routeInfos
                });
            }

            return View(leaderboard);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EcoDash.Services;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using EcoDash.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MongoDB.Driver.GridFS;

namespace EcoDash.Controllers
{
    [Authorize]
    public class RouteController : Controller
    {
        private readonly RouteService _routeService;
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoCollection<BsonDocument> _usercollection;
        private readonly GridFSBucket _gridFSBucket;

        // Combined constructor
        public RouteController(RouteService routeService, MongoDbContext dbContext)
        {
            _routeService = routeService;
            _collection = dbContext.GetCollection<BsonDocument>("Routes");
            _usercollection = dbContext.GetCollection<BsonDocument>("Members");
            _gridFSBucket = new GridFSBucket(dbContext.Database, new GridFSBucketOptions
            {
                BucketName = "Images"  // Prefix the collection with "Documents"
            });
        }

        [HttpGet]
        public IActionResult Index()
        {
            var googleMapsApiKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_KEY");
            ViewData["GoogleMapsApiKey"] = googleMapsApiKey;

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

            var googleMapsApiKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_KEY");
            ViewData["GoogleMapsApiKey"] = googleMapsApiKey;

            return View("RouteResults", routesData);
        }
        [HttpPost]
        public async Task<IActionResult> SaveRoute()
        {

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                Console.WriteLine("Raw JSON received: " + body);

                try
                {
                    string GUID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

                    if (string.IsNullOrEmpty(GUID))
                    {
                        return Unauthorized("User identifier not found.");

                    }

                    // Parse the raw JSON into a BsonDocument
                    var bsonDocument = BsonDocument.Parse(body);

                    // Extract route details from BSON (this assumes certain fields exist in your JSON)
                    double distanceKm = bsonDocument["legs"][0]["distance"]["value"].ToDouble() / 1000; // Convert meters to kilometers
                    string travelMode = bsonDocument["legs"][0]["steps"][0]["travel_mode"].ToString().ToLower();

                    // Calculate CO2 saved and calories burned
                    double co2Saved = 0.0;
                    double caloriesBurned = 0.0;

                    if (travelMode == "walking" || travelMode == "bicycling")
                    {
                        co2Saved = distanceKm * 0.27; // CO2 saved compared to driving
                        caloriesBurned = (travelMode == "walking") ? distanceKm * 60 : distanceKm * 30;
                    }
                    else if (travelMode == "transit")
                    {
                        co2Saved = distanceKm * 0.15; // Transit has lower emissions than driving
                    }

                    // Add these calculated values to the BSON document
                    bsonDocument.Add("co2Saved", co2Saved);
                    bsonDocument.Add("caloriesBurned", caloriesBurned);
                    bsonDocument.Add("GUID", GUID);
                    bsonDocument.Add("Completed", false);

                    // Insert the BsonDocument into MongoDB
                    _collection.InsertOne(bsonDocument);

                    return Ok(new { message = "Route saved successfully!", co2Saved, caloriesBurned });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error inserting route: " + ex.Message);
                    return StatusCode(500, "Error saving route to MongoDB.");
                }
            }
        }

        [HttpGet]
        public IActionResult ActiveRoutes()
        {
            string GUID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (string.IsNullOrEmpty(GUID))
            {
                return Unauthorized("User identifier not found.");
            }

            // Fetch all routes associated with this user from MongoDB
            var filter = Builders<BsonDocument>.Filter.Eq("GUID", GUID);
            var routes = _collection.Find(filter).ToList();

            // Pass the routes to the view
            return View(routes);  // Assuming you have a view to display the routes
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsDone(IFormFile routeImage, string routeId)
        {
            string GUID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (routeImage == null || string.IsNullOrEmpty(routeId))
            {
                return BadRequest("Image or Route ID is missing.");
            }

            try
            {
                // Prepare metadata for the image (e.g., GUID, RouteID)
                var metadata = new BsonDocument
        {
            { "GUID", GUID },
            { "RouteID", routeId },
            { "FileName", routeImage.FileName },
            { "UploadDate", DateTime.Now }
        };

                // Upload the image to MongoDB GridFS with metadata
                using (var stream = routeImage.OpenReadStream())
                {
                    var fileId = await _gridFSBucket.UploadFromStreamAsync(routeImage.FileName, stream, new GridFSUploadOptions
                    {
                        Metadata = metadata
                    });

                    Console.WriteLine("Image uploaded with file ID: " + fileId.ToString());
                }

                // Retrieve the route from the database
                var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(routeId));
                var route = _collection.Find(filter).FirstOrDefault();
                if (route == null)
                {
                    return NotFound("Route not found.");
                }

                // Mark the route as completed
                var update = Builders<BsonDocument>.Update.Set("Completed", true);
                _collection.UpdateOne(filter, update);

                // Calculate points for the completed route
                int routePoints = CalculatePoints(route);


                UpdateUserMetrics(GUID, route, routePoints);

                return Ok(new { message = "Route marked as done and points added!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error marking route as done: " + ex.Message);
                return StatusCode(500, "Error processing the request.");
            }
        }





        private void UpdateUserMetrics(string userId, BsonDocument route, int routePoints)
        {
            // Extract metrics from the route
            double co2Saved = route["co2Saved"].ToDouble();
            double caloriesBurned = route["caloriesBurned"].ToDouble();

            // Calculate eco-score (e.g., percentage of possible eco points - simplified)
            double ecoScore = (co2Saved * 5 + caloriesBurned / 10) / routePoints * 100;

            // Fetch the user's profile from MongoDB
            var filter = Builders<BsonDocument>.Filter.Eq("GUID", userId);
            var user = _usercollection.Find(filter).FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine("User not found for GUID: " + userId); // Debugging

                return;
            }
            Console.WriteLine("Route Points: " + routePoints);
            Console.WriteLine("CO2 Saved: " + co2Saved);
            Console.WriteLine("Calories Burned: " + caloriesBurned);

            // Update user metrics
            var update = Builders<BsonDocument>.Update
                .Inc("totalGreenCommutes", 1)                        // Increment total green commutes
                .Inc("totalCo2Saved", co2Saved)                      // Increment total CO2 saved
                .Inc("totalCaloriesBurned", caloriesBurned)          // Increment total calories burned
                .Inc("totalPoints", routePoints)                     // Increment total points
                .Set("topEcoScore", ecoScore);                       // Set new eco-score

            var updateResult = _usercollection.UpdateOne(filter, update);

            Console.WriteLine("Matched count: " + updateResult.MatchedCount);  // Should be 1 if the user was found
            Console.WriteLine("Modified count: " + updateResult.ModifiedCount);  // Should be 1 if the update was applied

        }
        private int CalculatePoints(BsonDocument route)
        {
            // Extract distance in kilometers
            double distanceKm = route["legs"][0]["distance"]["value"].ToDouble() / 1000;

            // Get CO2 saved and calories burned from the route
            double co2Saved = route["co2Saved"].ToDouble();
            double caloriesBurned = route["caloriesBurned"].ToDouble();

            // Calculate points based on distance, CO2 saved, and calories burned
            int distancePoints = (int)(distanceKm * 10);       // Example: 10 points per km
            int co2Points = (int)(co2Saved * 5);               // Example: 5 points per kg of CO2 saved
            int caloriePoints = (int)(caloriesBurned / 10);    // Example: 1 point per 10 calories burned

            // Total points for the route
            int totalPoints = distancePoints + co2Points + caloriePoints;

            return totalPoints;
        }

        [HttpGet]
        public async Task<IActionResult> GetRouteImage(string routeId)
        {
            if (string.IsNullOrEmpty(routeId))
            {
                return NotFound("Route ID not provided.");
            }

            try
            {
                // Create a filter to search for the file with the given RouteID in the metadata
                var filter = Builders<GridFSFileInfo>.Filter.Eq("metadata.RouteID", routeId);

                // Find the file information in GridFS
                var fileInfo = await _gridFSBucket.Find(filter).FirstOrDefaultAsync();

                if (fileInfo == null)
                {
                    return NotFound("Image not found for the given Route ID.");
                }

                // Download the image from GridFS
                var memoryStream = new MemoryStream();
                await _gridFSBucket.DownloadToStreamAsync(fileInfo.Id, memoryStream);

                memoryStream.Position = 0;

                // Return the image as a file result
                return File(memoryStream, "image/jpg");  // Adjust content type as needed
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching image: " + ex.Message);
                return NotFound();
            }
        }


    }
}

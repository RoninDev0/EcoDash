using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EcoDash.Services
{
    public class RouteService
    {
        private readonly HttpClient _httpClient;

        public RouteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JObject> GetEcoFriendlyRoutes(string startLocation, string destination, string mode)
        {
            // Replace with your Google Maps API Key
            string googleMapsApiKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_KEY");

            // Ensure the API URL includes the selected mode of transportation
            string requestUrl = $"https://maps.googleapis.com/maps/api/directions/json?origin={startLocation}&destination={destination}&mode={mode}&alternatives=true&key={googleMapsApiKey}";

            var response = await _httpClient.GetAsync(requestUrl);
            var jsonData = await response.Content.ReadAsStringAsync();

            // Log the raw JSON response for debugging purposes
            Console.WriteLine("JSONDATA: " + jsonData);

            return JObject.Parse(jsonData);
        }
    }
}


using EcoDash.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EcoDash.Data
{
    public class MongoDbUtils
    {
        public static (bool, string) SignupUser(SignUpModel Model, IMongoCollection<BsonDocument> Users)
        {
            try
            {
                // Check if the username already exists in the database
                var filter = Builders<BsonDocument>.Filter.Eq("Username", Model.Username);
                var existingUser = Users.Find(filter).FirstOrDefault();

                if (existingUser != null)
                {
                    // Username already exists, return failure
                    return (false, "Username already exists");
                }

                // Generate a random GUID for the new user
                var newGuid = Guid.NewGuid().ToString();

                // Hash the password using BCrypt
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Model.Password);

                // Create a new BSON document to represent the new user
                var newUser = new BsonDocument
        {
            { "Username", Model.Username },
            { "Password", hashedPassword },
            { "GUID", newGuid },
            { "CreatedAt", DateTime.UtcNow } // Optionally store the creation date
        };

                // Insert the new user into the collection
                Users.InsertOne(newUser);

                // Return success with the new GUID
                return (true, newGuid);
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Return failure by default
            return (false, "Signup failed");
        }
        public static (bool, string) LoginUser(LoginModel Model, IMongoCollection<BsonDocument> Users)
        {
            try
            {
                // Find the user by username
                var filter = Builders<BsonDocument>.Filter.Eq("Username", Model.Username);
                var channel = Users.Find(filter).FirstOrDefault();

                if (channel == null)
                {
                    // User not found
                    return (false, null);
                }

                // Get the hashed password stored in the database
                var storedHashedPassword = channel.GetValue("Password").AsString;

                // Verify the entered password against the hashed password using bcrypt
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(Model.Password, storedHashedPassword);

                if (isPasswordCorrect)
                {
                    // If the password is correct, return success and the GUID
                    return (true, channel.GetValue("GUID").AsString);
                }

                // If password is incorrect, return failure
                return (false, null);
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return (false, null); // Return failure by default
        }
    }
}

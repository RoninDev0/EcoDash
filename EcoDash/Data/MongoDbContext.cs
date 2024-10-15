using MongoDB.Bson;
using MongoDB.Driver;

namespace EcoDash.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        // Non-generic version (for BsonDocument)
        public IMongoCollection<BsonDocument> GetCollection(string collectionName)
        {
            return _database.GetCollection<BsonDocument>(collectionName);
        }

        // Generic version (for strongly typed collections)
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        public IMongoDatabase Database => _database; // Use for GridFSBucket or other custom operations
    }


}

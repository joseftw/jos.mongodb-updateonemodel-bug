using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var settings = MongoClientSettings.FromConnectionString($"mongodb://username:MyPassword@localhost");
            settings.ConnectTimeout = TimeSpan.FromSeconds(5);
            var client = new MongoClient(settings);

            await client.DropDatabaseAsync("test");
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<MongoDbSomeModel>("test");

            var bulkOperations = new List<WriteModel<MongoDbSomeModel>>();
            AddUpdateOneModels(bulkOperations);
            AddInsertOneModels(bulkOperations);

            await collection.BulkWriteAsync(bulkOperations);

            var baseCollection = database.GetCollection<MongoDbBaseModel>("test");
            var items = await baseCollection.Find(x => true).ToListAsync();
            Console.WriteLine($"Fetched {items.Count} items");
            foreach (var item in items)
            {
                switch (item)
                {
                    case MongoDbSomeModel someModel:
                        Console.WriteLine($"Id: {someModel.Id} - Type: {someModel.GetType().Name}");
                        break;
                    default:
                        throw new Exception($"We don't support '{item.GetType().Name}'");
                }
            }
        }

        private static void AddUpdateOneModels(List<WriteModel<MongoDbSomeModel>> bulkOperations)
        {
            var items = new List<MongoDbSomeModel>
            {
                new MongoDbSomeModel{Id = "1", Name = "Name 1 UPSERT", SomeProperty = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")},
                new MongoDbSomeModel{Id = "2", Name = "Name 2 UPSERT", SomeProperty = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")},
                new MongoDbSomeModel{Id = "3", Name = "Name 3 UPSERT", SomeProperty = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")}
            };
            foreach (var record in items)
            {
                var updateDefinition = new UpdateDefinitionBuilder<MongoDbSomeModel>()
                    .Set("_t", StandardDiscriminatorConvention.Hierarchical.GetDiscriminator(typeof(MongoDbBaseModel), typeof(MongoDbSomeModel)))
                    .Set(x => x.Name, record.Name)
                    .Set(x => x.SomeProperty, record.SomeProperty);
                var updateOperation = new UpdateOneModel<MongoDbSomeModel>(
                    Builders<MongoDbSomeModel>.Filter.Eq(x => x.Id, record.Id),
                    updateDefinition)
                {
                    IsUpsert = true
                };

                bulkOperations.Add(updateOperation);
            }
        }

        private static void AddInsertOneModels(List<WriteModel<MongoDbSomeModel>> bulkOperations)
        {
            var items = new List<MongoDbSomeModel>
            {
                new MongoDbSomeModel{Id = "4", Name = "Name 4 INSERT", SomeProperty = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")},
                new MongoDbSomeModel{Id = "5", Name = "Name 5 INSERT", SomeProperty = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")},
                new MongoDbSomeModel{Id = "6", Name = "Name 6 INSERT", SomeProperty = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")}
            };

            foreach (var record in items)
            {
                var insertOperation = new InsertOneModel<MongoDbSomeModel>(record);
                bulkOperations.Add(insertOperation);
            }
        }
    }
}

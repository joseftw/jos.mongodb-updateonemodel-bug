using MongoDB.Bson.Serialization.Attributes;

namespace ConsoleApp1
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(MongoDbSomeModel))]
    [BsonIgnoreExtraElements]
    public class MongoDbBaseModel
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class MongoDbSomeModel : MongoDbBaseModel
    {
        public string SomeProperty { get; set; }
    }
}

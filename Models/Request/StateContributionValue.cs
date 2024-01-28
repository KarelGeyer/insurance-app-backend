using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models.Request
{
    public class StateContributionValue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userContribution")]
        [BsonRepresentation(BsonType.Int32)]
        public int UserContribution { get; set; }

        [BsonElement("stateContribution")]
        [BsonRepresentation(BsonType.Int32)]
        public int StateContribution { get; set; }
    }
}

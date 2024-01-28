using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using insurance_backend.Enums;

namespace insurance_backend.Models.Request
{
	public class StrategyInterestRate
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("interestRate")]
		[BsonRepresentation(BsonType.Int32)]
		public int InterestRate { get; set; }

		[BsonElement("name")]
		[BsonRepresentation(BsonType.String)]
		public string Name { get; set; } = String.Empty;
	}
}

using insurance_backend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models
{
	public class PensionProduct
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("productId")]
		[BsonRepresentation(BsonType.String)]
		public string ProductId { get; set; }

		[BsonElement("name")]
		[BsonRepresentation(BsonType.String)]
		public string Name { get; set; }

		[BsonElement("dynamic")]
		[BsonRepresentation(BsonType.Double)]
		public double DynamicPercentage { get; set; }

		[BsonElement("conservative")]
		[BsonRepresentation(BsonType.Double)]
		public double ConservativePercentage { get; set; }

		[BsonElement("balanced")]
		[BsonRepresentation(BsonType.Double)]
		public double BalancedPercentage { get; set; }
	}
}

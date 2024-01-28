using insurance_backend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models
{
	public class Order
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("productId")]
		[BsonRepresentation(BsonType.String)]
		public string ProductId { get; set; }

		[BsonElement("productName")]
		[BsonRepresentation(BsonType.String)]
		public string ProductName { get; set; }

		[BsonElement("name")]
		[BsonRepresentation(BsonType.String)]
		public string Name { get; set; }

		[BsonElement("surname")]
		[BsonRepresentation(BsonType.String)]
		public string Surname { get; set; }

		[BsonElement("productCategory")]
		[BsonRepresentation(BsonType.Int32)]
		public ProductCategory Category { get; set; }

		[BsonElement("date")]
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime Date { get; set; }

		[BsonElement("yearlyPrice")]
		[BsonRepresentation(BsonType.Double)]
		public double YearlyPrice { get; set; }
	}
}

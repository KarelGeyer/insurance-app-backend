using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models
{
	public class ProductInsuranceProduct
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

		[BsonElement("houseCoefficient")]
		[BsonRepresentation(BsonType.Double)]
		public double HousePerMeterSqaureCoefficient { get; set; }

		[BsonElement("flatCoefficient")]
		[BsonRepresentation(BsonType.Double)]
		public double FlatPerMeterSqaureCoefficient { get; set; }

		[BsonElement("garageCoefficient")]
		[BsonRepresentation(BsonType.Double)]
		public double GaragePerMeterSqaureCoefficient { get; set; }

		[BsonElement("equipmentCoefficient")]
		[BsonRepresentation(BsonType.Double)]
		public double EquipmentCoefficient { get; set; }

		[BsonElement("liabilityCoefficient")]
		[BsonRepresentation(BsonType.Double)]
		public double LiabilityCoefficient { get; set; }
	}
}

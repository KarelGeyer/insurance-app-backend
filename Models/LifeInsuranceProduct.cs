using insurance_backend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models
{
	public class LifeInsuranceProduct
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

		[BsonElement("deathPrice")]
		[BsonRepresentation(BsonType.Double)]
		public double DeathCoefficient { get; set; }

		[BsonElement("injuriesPrice")]
		[BsonRepresentation(BsonType.Double)]
		public double InjuriesCoefficient { get; set; }

		[BsonElement("diseasesPrice")]
		[BsonRepresentation(BsonType.Double)]
		public double DiseasesCoefficient { get; set; }

		[BsonElement("workIncapacityPrice")]
		[BsonRepresentation(BsonType.Double)]
		public double WorkIncapacityCoefficient { get; set; }

		[BsonElement("hospitalizationPrice")]
		[BsonRepresentation(BsonType.Double)]
		public double HospitalizationCoefficient { get; set; }

		[BsonElement("invalidityPrice")]
		[BsonRepresentation(BsonType.Double)]
		public double InvalidityCoefficient { get; set; }

		[BsonElement("smokerPercentage")]
		[BsonRepresentation(BsonType.Double)]
		public double SmokerCoefficient { get; set; }

		[BsonElement("sportPercentageNegavitve")]
		[BsonRepresentation(BsonType.Double)]
		public double SportCoefficient { get; set; }

		[BsonElement("sportPercentagePositive")]
		[BsonRepresentation(BsonType.Double)]
		public double SportCoefficientP { get; set; }
	}
}

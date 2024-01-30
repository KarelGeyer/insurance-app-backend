using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models.Request.Product
{
	public class PropertyInsuranceProductCreateRequest : ProductBase
	{
		public double HousePerMeterSqaureCoefficient { get; set; }

		public double FlatPerMeterSqaureCoefficient { get; set; }

		public double GaragePerMeterSqaureCoefficient { get; set; }

		public double EquipmentCoefficient { get; set; }

		public double LiabilityCoefficient { get; set; }
	}
}

using insurance_backend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models.Request.Product
{
	public class LifeInsuranceProductCreateRequest : ProductBase
	{
		public double DeathCoefficient { get; set; }

		public double InjuriesCoefficient { get; set; }

		public double DiseasesCoefficient { get; set; }

		public double WorkIncapacityCoefficient { get; set; }

		public double HospitalizationCoefficient { get; set; }

		public double InvalidityCoefficient { get; set; }

		public double SmokerCoefficient { get; set; }

		public double SportCoefficient { get; set; }

		public double SportCoefficientP { get; set; }
	}
}

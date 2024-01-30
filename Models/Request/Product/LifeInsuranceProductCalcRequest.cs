using insurance_backend.Enums;

namespace insurance_backend.Models.Request.Product
{
	public class LifeInsuranceProductCalcRequest
	{
		public string ProductId { get; set; } = string.Empty;

		public string? Email { get; set; } = string.Empty;

		public int DeathInsurance { get; set; }

		public int InjuriesInsurance { get; set; }

		public int DiseasesInsurance { get; set; }

		public int WorkIncapacityInsurance { get; set; }

		public int HospitalizationInsurance { get; set; }

		public int InvalidityInsurance { get; set; }

		public int HospitalizationLength { get; set; }

		public InvalidityLevel InvalidityLevel { get; set; }

		public bool IsSmoker { get; set; }

		public bool DoesSport { get; set; }
	}
}

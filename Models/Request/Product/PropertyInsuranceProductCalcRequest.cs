using insurance_backend.Enums;

namespace insurance_backend.Models.Request.Product
{
	public class PropertyInsuranceProductCalcRequest
	{
		public string ProductId { get; set; } = string.Empty;

		public string? Email { get; set; } = string.Empty;

		public string PropertyType { get; set; } = string.Empty;

		public string Street { get; set; } = string.Empty;

		public string City { get; set; } = string.Empty;

		public string ZipCode { get; set; } = string.Empty;

		public int SquareMeters { get; set; }

		public bool ShouldCalculateProperty { get; set; }

		public bool ShouldCalculateEquipment { get; set; }

		public bool ShouldCalculateLiability { get; set; }
	}
}

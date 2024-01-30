using insurance_backend.Enums;

namespace insurance_backend.Models.Request.Product
{
	public class ProductBase
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string CompanyName { get; set; }

		public string CompanyLogo { get; set; }

		public ProductCategory Category { get; set; }
	}
}

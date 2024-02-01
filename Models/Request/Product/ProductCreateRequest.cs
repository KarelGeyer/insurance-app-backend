using insurance_backend.Enums;
using MongoDB.Bson;

namespace insurance_backend.Models.Request.Product
{
	public class ProductCreateRequest : ProductBase
	{
		public string Id { get; set; }

		public ProductCategory Category { get; set; }
	}
}

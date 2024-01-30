using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models.Request.Product
{
	public class PensionProductCreateRequest : ProductBase
	{
		public string ProductId { get; set; }

		public double DynamicPercentage { get; set; }

		public double ConservativePercentage { get; set; }

		public double BalancedPercentage { get; set; }
	}
}

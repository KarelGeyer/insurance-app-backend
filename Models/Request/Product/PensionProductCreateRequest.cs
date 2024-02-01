using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using insurance_backend.Enums;
using Newtonsoft.Json;

namespace insurance_backend.Models.Request.Product
{
	public class PensionProductCreateRequest : ProductBase
	{
		public double DynamicPercentage { get; set; }

		public double ConservativePercentage { get; set; }

		public double BalancedPercentage { get; set; }
	}
}

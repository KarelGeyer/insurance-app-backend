using insurance_backend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace insurance_backend.Models.Request.Order
{
	public class OrderCreateRequest
	{
		public string ProductId { get; set; }

		public string Name { get; set; }

		public string Surname { get; set; }

		public DateTime Date { get; set; }

		public double YearlyPrice { get; set; }

		public string EmailAddress { get; set; }
	}
}

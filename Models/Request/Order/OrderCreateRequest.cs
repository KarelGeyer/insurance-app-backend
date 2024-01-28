namespace insurance_backend.Models.Request.Order
{
	public class OrderCreateRequest
	{
		public insurance_backend.Models.Order Order { get; set; }
		public string EmailAddress { get; set; }
	}
}

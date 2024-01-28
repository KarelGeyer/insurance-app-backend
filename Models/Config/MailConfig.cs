namespace insurance_backend.Models.Config
{
	public class MailConfig
	{
		public string Address { get; set; }
		public string Smtp { get; set; }
		public int Port { get; set; }
		public bool ShouldUseSSL { get; set; }
		public string SSL { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}

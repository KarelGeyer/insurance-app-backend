using insurance_backend.Models.Config;
using insurance_backend.Services;
using MimeKit;

namespace insurance_backend.Interfaces
{
	public interface IEmailService
	{
		public void SendEmail(string body, string subject, string to);
	}
}

using insurance_backend.Models.Config;
using insurance_backend.Models.Db;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using MimeKit;
using insurance_backend.Interfaces;
using System.Text.Encodings.Web;

namespace insurance_backend.Services
{
	public class EmailService : IEmailService
	{
		private MailConfig _config;
		private MailboxAddress _emailAddress;
		private ILogger<EmailService> _logger;

		public EmailService(IOptions<MailConfig> config, ILogger<EmailService> logger)
		{
			_config = config.Value;
			_emailAddress = MailboxAddress.Parse(_config.Address);
			_logger = logger;
		}

		public void SendEmail(string body, string subject, string to)
		{
			_logger.LogInformation($"{nameof(SendEmail)} - Start");
			MimeMessage email = new();

			email.From.Add(_emailAddress);
			email.To.Add(MailboxAddress.Parse(to));
			email.Subject = subject;
			email.Body = new TextPart(TextFormat.Html) { Text = body };
			_logger.LogInformation($"{nameof(SendEmail)} - email created and configured, attempting to create a client");
			SmtpClient client = new();

			_logger.LogInformation($"{nameof(SendEmail)} - client created, attempting to connect and send email");
			try
			{
				client.Connect(_config.Smtp, _config.Port, _config.ShouldUseSSL);
				client.Authenticate(_config.Username, _config.Password);
				client.Send(email);
				_logger.LogInformation($"{nameof(SendEmail)} - email sent successfully");
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"{nameof(SendEmail)} - email could have not been sent");
				_logger.LogError(ex.Message);
				throw new Exception(ex.Message);
			}

			client.Disconnect(true);
			client.Dispose();
		}
	}
}

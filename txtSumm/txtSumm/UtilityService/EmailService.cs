using MailKit.Net.Smtp;
using MimeKit;
using txtSumm.Models;

namespace txtSumm.UtilityService
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _config;
		public EmailService(IConfiguration configuration)
		{
			_config = configuration;
		}

		public void SendEmail(Email email)
		{
			var emailMessage = new MimeMessage();
			var from = _config["EmailSettings:From"];
			emailMessage.From.Add(new MailboxAddress("Changing Password", from));
			emailMessage.To.Add(new MailboxAddress(email.To, email.To));
			emailMessage.Subject = email.Subject;
			emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
			{
				Text = string.Format(email.Content)
			};
			using (var client = new SmtpClient())
			{
				try
				{
					client.Connect(_config["EmailSettings:SmtpServer"], 465, true);
					client.Authenticate(_config["EmailSettings:From"], _config["EmailSettings:Password"]);
					client.Send(emailMessage);
				}
				catch (Exception ex)
				{
					throw;
				}
				finally
				{
					client.Disconnect(true);
					client.Dispose();
				}
			}
		}
	}
}

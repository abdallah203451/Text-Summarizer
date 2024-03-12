using txtSumm.Models;

namespace txtSumm.UtilityService
{
	public interface IEmailService
	{
		void SendEmail(Email email);
	}
}

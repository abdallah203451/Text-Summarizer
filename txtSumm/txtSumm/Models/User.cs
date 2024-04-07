using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace txtSumm.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		[EmailAddress]
		public string Email { get; set; }

		public string Phone { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public string Token { get; set; }

		public string Role { get; set; }

		public string RefreshToken { get; set; }

		public DateTime RefreshTokenExpiryTime { get; set; }

		public string ResetPasswordToken { get; set; }

		public DateTime ResetPasswordExpiry { get; set; }

		[ValidateNever]
		public List<History> History { get; set; }
	}
}

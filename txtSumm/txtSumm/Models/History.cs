using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;

namespace txtSumm.Models
{
	public class History
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public string TextSummary { get; set; }
		public DateTime Date { get; set; }
		public int UserId { get; set; }

		//Navigation Property
		[ValidateNever]
		public User User { get; set; }
	}
}

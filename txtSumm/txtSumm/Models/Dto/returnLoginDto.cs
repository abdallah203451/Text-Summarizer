namespace txtSumm.Models.Dto
{
	public class returnLoginDto
	{
		public string Email { get; set; }
		public string Phone { get; set; }
		public string UserName { get; set; }
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
	}
}

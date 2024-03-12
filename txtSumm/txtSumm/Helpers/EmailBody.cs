namespace txtSumm.Helpers
{
	public class EmailBody
	{
		public static string EmailStringBody(string email, string emailToken)
		{
			return $@"<html>
						<head></head>
							<body style=""margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif;"">
    <div style=""height: auto; background: linear-gradient(to top, #c9c9ff 50%, #6e6ef6 90%) no-repeat; width: 400px; padding: 20px; margin: 0 auto;"">
        <h1>Reset Your Password</h1>
        <hr>
        <p>You're receiving this email because you requested a password reset for your Txt Summerizer account.</p>
        <p>Please click the button below to choose a new password:</p>
        <a href=""http://localhost:60045/reset?email={email}&code={emailToken}"" target=""_blank"" style=""background: #007bff; color: white; border-radius: 4px; display: block; margin: 0 auto; width: 50%; text-align: center; text-decoration: none; padding: 10px;"">
            Reset Password
        </a>
        <p>Kind Regards,<br><br>ConnectX</p>
    </div>
</body>
</html>";
		}
	}
}

using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using txtSumm.Context;
using txtSumm.Helpers;
using txtSumm.Migrations;
using txtSumm.Models;
using txtSumm.Models.Dto;
using txtSumm.UtilityService;
namespace txtSumm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
	{
		private readonly AppDbContext _authContext;
		private readonly IConfiguration _configuration;
		private readonly UtilityService.IEmailService _emailService;
		public UserController(AppDbContext appDbContext, IConfiguration configuration, UtilityService.IEmailService emailService)
		{
			_authContext = appDbContext;
			_configuration = configuration;
			_emailService = emailService;
		}


		[HttpPost("login")]
		public async Task<IActionResult> Authenticate([FromBody] User userObj)
		{
			if (userObj == null)
				return BadRequest();

			var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == userObj.Email);

			if (user == null)
				return NotFound(new
				{
					Message = "Email Not Found!"
				});

			if (!HashingPassword.VerifyPassword(userObj.Password, user.Password))
				return BadRequest(new
				{
					message = "Password is incorrect"
				});

			user.Token = CreateJwt(user);
			var newAccessToken = user.Token;
			var newRefreshToken = CreateRefreshToken();
			user.RefreshToken = newRefreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1);
			await _authContext.SaveChangesAsync();


			return Ok(new returnLoginDto()
			{
				Email = user.Email,
				Phone = user.Phone,
				UserName = user.UserName,
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken
			});

			//return Ok(new TokenApiDto()
			//{
			//	AccessToken = newAccessToken,
			//	RefreshToken = newRefreshToken
			//});

			//return Ok("successful login");
		}

		[HttpPost("register")]
		public async Task<IActionResult> RegisterUser([FromBody] User userObj)
		{
			if (userObj == null)
				return BadRequest();

			//Check if username exist
			//if (await CheckUserNameExistAsync(userObj.Username))
			//	return BadRequest(new
			//	{
			//		Message = "Username Already Exist!"
			//	});


			//Check if Email exist
			if (await CheckEmailExistAsync(userObj.Email))
				return BadRequest(new
				{
					Message = "Email Already Exist!"
				});


			userObj.Password = HashingPassword.HashPassword(userObj.Password);

			userObj.Role = "User";
			userObj.Token = "";
			userObj.Token = CreateJwt(userObj);
			var newAccessToken = userObj.Token;
			var newRefreshToken = CreateRefreshToken();
			userObj.RefreshToken = newRefreshToken;
			userObj.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1);
			// Add the data to the database and save the changes
			await _authContext.Users.AddAsync(userObj);
			await _authContext.SaveChangesAsync();
			return Ok(new returnLoginDto()
			{
				Email = userObj.Email,
				Phone = userObj.Phone,
				UserName = userObj.UserName,
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken
			});
		}

		[Authorize]
		[HttpPut("edituser")]
		public async Task<IActionResult> EditProfile([FromBody] User userObj)
		{
			var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == userObj.Email);

			if (user == null)
				return NotFound(new
				{
					Message = "user not found"
				});

			user.Phone = userObj.Phone;
			user.UserName = userObj.UserName;

			_authContext.Users.Update(user);
			_authContext.SaveChanges();

			return Ok(new
			{
				Message = "Edited Successfully"
			});
		}

		[Authorize]
		[HttpGet("getuser")]
		public async Task<IActionResult> GetUser(User userObj)
		{
			var user = await _authContext.Users.FirstOrDefaultAsync(a => a.Email == userObj.Email);
			if (user is null)
			{
				return NotFound(new
				{
					StatusCode = 404,
					Message = "User Doesn't Exist"
				});
			}
			return Ok(new returnLoginDto()
			{
				Email = user.Email,
				Phone = user.Phone,
				UserName = user.UserName,
				AccessToken = "",
				RefreshToken = ""
			});
		}


		//private Task<bool> CheckUserNameExistAsync(string username) => _authContext.Users.AnyAsync(x => x.Username == username);

		private Task<bool> CheckEmailExistAsync(string email) => _authContext.Users.AnyAsync(x => x.Email == email);

		private string CreateJwt(User user)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes("verysecret.....verysecret.....verysecret.....verysecret.....");
			var identity = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.Role, user.Role),
				new Claim(ClaimTypes.Name,$"{user.Email}")
			});

			var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = identity,
				Expires = DateTime.UtcNow.AddHours(1),
				SigningCredentials = credentials
			};
			var token = jwtTokenHandler.CreateToken(tokenDescriptor);
			return jwtTokenHandler.WriteToken(token);
		}

		private string CreateRefreshToken()
		{
			var tokenBytes = RandomNumberGenerator.GetBytes(64);
			var refreshToken = Convert.ToBase64String(tokenBytes);

			var tokenInUser = _authContext.Users
				.Any(a => a.RefreshToken == refreshToken);
			if (tokenInUser)
			{
				return CreateRefreshToken();
			}
			return refreshToken;
		}

		private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
		{
			var key = Encoding.ASCII.GetBytes("verysecret.....");
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateLifetime = false
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			SecurityToken securityToken;
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
			var jwtSecurityToken = securityToken as JwtSecurityToken;
			if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				throw new SecurityTokenException("This is Invalid Token");
			return principal;

		}


		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
		{
			if (tokenApiDto is null)
				return BadRequest("Invalid Client Request");
			string accessToken = tokenApiDto.AccessToken;
			string refreshToken = tokenApiDto.RefreshToken;
			var principal = GetPrincipleFromExpiredToken(accessToken);
			var username = principal.Identity.Name;
			var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Email == username);
			if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
				return BadRequest("Invalid Request");
			var newAccessToken = CreateJwt(user);
			var newRefreshToken = CreateRefreshToken();
			user.RefreshToken = newRefreshToken;
			await _authContext.SaveChangesAsync();
			return Ok(new TokenApiDto()
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken,
			});
		}

		[HttpPost("send-reset-email/{email}")]
		public async Task<IActionResult> SendEmail(string email)
		{
			var user = await _authContext.Users.FirstOrDefaultAsync(a => a.Email == email);
			if (user is null)
			{
				return NotFound(new
				{
					StatusCode = 404,
					Message = "email Doesn't Exist"
				});
			}
			var tokenBytes = RandomNumberGenerator.GetBytes(64);
			var emailToken = Convert.ToBase64String(tokenBytes);
			user.ResetPasswordToken = emailToken;
			user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
			string from = _configuration["EmailSettings:From"];
			var emailModel = new Email(email, "Reset Password!", EmailBody.EmailStringBody(email, emailToken));
			_emailService.SendEmail(emailModel);
			_authContext.Entry(user).State = EntityState.Modified;
			await _authContext.SaveChangesAsync();
			return Ok(new
			{
				StatusCode = 200,
				Message = "Email Sent!"
			});
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
		{
			var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
			var user = await _authContext.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
			if (user is null)
			{
				return NotFound(new
				{
					StatusCode = 404,
					Message = "email Doesn't Exist"
				});
			}
			var tokenCode = user.ResetPasswordToken;
			DateTime emailTokenExpiry = user.ResetPasswordExpiry;
			if (tokenCode != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
			{
				return BadRequest(new
				{
					StatusCode = 400,
					Message = "Inavalid Reset link"
				});
			}
			user.Password = HashingPassword.HashPassword(resetPasswordDto.NewPassword);
			_authContext.Entry(user).State = EntityState.Modified;
			await _authContext.SaveChangesAsync();
			return Ok(new
			{
				StatusCode = 200,
				Message = "Password Reset Successfully"
			});
		}
	}
}

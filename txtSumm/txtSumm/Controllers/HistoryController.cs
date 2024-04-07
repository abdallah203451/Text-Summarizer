using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using txtSumm.Context;
using txtSumm.Models;
using txtSumm.Models.Dto;

namespace txtSumm.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class HistoryController : ControllerBase
	{
		private readonly AppDbContext _context;
		public HistoryController(AppDbContext appDbContext)
		{
			_context = appDbContext;
		}


		[HttpPost("addtext")]
		public async Task<IActionResult> AddText([FromBody] addTextDto historyDto)
		{
			History history = new History();
			history.Text = historyDto.Text;
			history.TextSummary = historyDto.TextSummary;
			history.Date = DateTime.Now;
			User user = await _context.Users.FirstOrDefaultAsync(e => e.Email == historyDto.Email);
			int userId = user.Id;
			history.UserId = userId;
			await _context.History.AddAsync(history);
			await _context.SaveChangesAsync();

			return Ok();
		}

		[HttpGet("gethistory")]
		public async Task<ActionResult<IEnumerable<History>>> GetHistory(string email)
		{
			User user = await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
			if (user == null)
				return NotFound(new
				{
					Message = "Email Not Found!"
				});
			int userId = user.Id;
			var history = await _context.History.Where(e => e.UserId == userId).Select(x => new { x.Id, x.Text, x.TextSummary, x.Date }).ToListAsync();

			return Ok(history);
		}

		//delete
		[HttpDelete("delete")]
		public async Task<IActionResult> DeleteHistoryEntry(int id)
		{
			History history = await _context.History.FindAsync(id);
			if (history == null)
				return NotFound(new
				{
					Message = "history not found"
				});

			_context.History.Remove(history);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				Message = "Removed Successfully"
			});
		}

		//delete
		[HttpPut("edit")]
		public async Task<IActionResult> EditHistoryEntry(History hisObj)
		{
			History history = await _context.History.FindAsync(hisObj.Id);
			if (history == null)
				return NotFound(new
				{
					Message = "history not found"
				});

			history.Text = hisObj.Text;
			history.TextSummary = hisObj.TextSummary;
			history.Date = DateTime.Now;

			_context.History.Update(history);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				Message = "Edited Successfully"
			});
		}
	}
}
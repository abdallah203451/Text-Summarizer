using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using txtSumm.Context;
using txtSumm.Models;

namespace txtSumm.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HistoryController : ControllerBase
	{
		private readonly AppDbContext _context;
		public HistoryController(AppDbContext appDbContext)
		{
			_context = appDbContext;
		}


		[HttpPost("addtext")]
		public async Task<IActionResult> AddText([FromBody] History history)
		{
			await _context.History.AddAsync(history);
			await _context.SaveChangesAsync();

			return Ok();
		}

		[HttpGet("gethistory")]
		public async Task<ActionResult<IEnumerable<History>>> GetHistory(int userId)
		{
			List<History> history = await _context.History.Where(e => e.UserId == userId).ToListAsync();

			return Ok(history);
		}
	}
}

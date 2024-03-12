using Microsoft.EntityFrameworkCore;
using txtSumm.Models;

namespace txtSumm.Context
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{

		}

		public DbSet<User> Users { get; set; }
		public DbSet<History> History { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}

using Microsoft.EntityFrameworkCore;
using WebShared.Services;

namespace Models
{
    public class AppDbContext : DbContext
    {
        // Constructor that accepts DbContextOptions<AppDbContext> and passes it to the base class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Define your DbSets here
        //public DbSet<PUMP1> PUMP1 { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;

namespace GitHubReadmeWebTrends.Common
{
    public class OptOutDbContext : DbContext
    {
        public OptOutDbContext(DbContextOptions<OptOutDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<OptOutModel> OptOutDatabaseModel => Set<OptOutModel>();
    }
}

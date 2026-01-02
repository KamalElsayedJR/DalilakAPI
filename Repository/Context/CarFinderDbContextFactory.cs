using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Repository.Context
{
    public class CarFinderDbContextFactory : IDesignTimeDbContextFactory<CarFinderDbContext>
    {
        public CarFinderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CarFinderDbContext>();
            
            // Use the development database connection string for migrations
            optionsBuilder.UseSqlServer("Server=.;Database=CarFinderDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new CarFinderDbContext(optionsBuilder.Options);
        }
    }
}

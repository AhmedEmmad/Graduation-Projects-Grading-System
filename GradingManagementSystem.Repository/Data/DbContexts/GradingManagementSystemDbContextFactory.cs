using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GradingManagementSystem.Repository.Data.DbContexts
{
    public class GradingManagementSystemDbContextFactory : IDesignTimeDbContextFactory<GradingManagementSystemDbContext>
    {
        private readonly IConfiguration _configuration;

        public GradingManagementSystemDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public GradingManagementSystemDbContext CreateDbContext(string[] args)
        {

            var optionsBuilder = new DbContextOptionsBuilder<GradingManagementSystemDbContext>();
            var connectionString = _configuration.GetConnectionString("MonsterConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new GradingManagementSystemDbContext(optionsBuilder.Options);
        }
    }
}


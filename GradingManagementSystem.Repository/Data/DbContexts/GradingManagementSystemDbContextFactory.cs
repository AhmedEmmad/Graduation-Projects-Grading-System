using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GradingManagementSystem.Repository.Data.DbContexts
{
    public class GradingManagementSystemDbContextFactory : IDesignTimeDbContextFactory<GradingManagementSystemDbContext>
    {
        public GradingManagementSystemDbContextFactory()
        {
           
        }

        public GradingManagementSystemDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GradingManagementSystemDbContext>();

            optionsBuilder.UseSqlServer("Server = db15279.public.databaseasp.net; Database=db15279; User Id = db15279; Password=4z=RZ?2x%5yX; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;");

            return new GradingManagementSystemDbContext(optionsBuilder.Options);
        }
    }
}


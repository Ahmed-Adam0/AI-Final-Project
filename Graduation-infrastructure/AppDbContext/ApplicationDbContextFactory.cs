using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Graduation_infrastructure.AppDbContext
{
    // Design-time factory to allow EF tools to create ApplicationDbContext when running migrations
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var connectionString =
                configuration.GetConnectionString("GraduationDbOnline")
                ?? configuration["ConnectionStrings:GraduationDbOnline"]
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__GraduationDbOnline");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to LocalDB for development if no connection string provided
                connectionString =
                    "Server=db49972.public.databaseasp.net; Database=db49972; User Id=db49972; Password=qE-9D2h+d!3L; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

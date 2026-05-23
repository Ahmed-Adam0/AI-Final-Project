using System;
using System.Collections.Generic;
using System.Text;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Graduation_infrastructure.ProgramService.ServicesAPI
{
    public static class ServiceAPI
    {
        public static void AddInfrastructureAPI(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connectionString = configuration.GetConnectionString("GraduationDbOnline");

            //if (string.IsNullOrWhiteSpace(connectionString))
            //{
            //    connectionString =
            //        "Server=db49972.public.databaseasp.net; Database=db49972; User Id=db49972; Password=qE-9D2h+d!3L; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
            //    ;
            //}

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString).EnableSensitiveDataLogging()
            );

            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_Application.Mapper.ProductMapping;
using Graduation_Application.Services;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Graduation_infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Graduation_infrastructure.ProgramService.ServicesMVC
{
    public static class ServiceMVC
    {
        public static void AddInfrastructureMVC(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connectionString = configuration.GetConnectionString("GraduationDbOnline");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString).EnableSensitiveDataLogging()
            );

            services.AddDbContext<ApplicationDbContext>();

            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register Generic Repository
            services.AddScoped(typeof(IGenaricRepositories<>), typeof(GenaricRepositories<>));

            // Register Mapping Configurations
            ProductMappingConfig.RegisterMappings();

            // Register Admin Services
            services.AddScoped<IAdminProductService, AdminProductService>();
        }
    }
}


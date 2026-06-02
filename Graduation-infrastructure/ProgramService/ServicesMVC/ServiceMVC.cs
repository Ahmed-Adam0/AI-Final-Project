using System;
using System.Collections.Generic;
using System.Text;
using Graduation_Application.ExternalServices.EmailServices;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_Application.Mapper.ProductMapping;
using Graduation_Application.Options;
using Graduation_Application.Services;
using Graduation_Application.Services.Admin;
using Graduation_Application.IRepositories.Admin;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Graduation_infrastructure.Repositories;
using Graduation_infrastructure.Repositories.Admin;
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
            services.AddScoped<IAdminReviewService, AdminReviewService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAdminVendorsRepository, AdminVendorsRepository>();
            services.AddScoped<IAdminVendorsService, AdminVendorsService>();

            // Register Application Services required by OrderService and related flows
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();
            services.Configure<WhatsAppNotificationSettings>(
                configuration.GetSection("WhatsAppNotification")
            );
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IInternalNotificationService, InternalNotificationService>();
            services.AddScoped<IInternalNotificationRepository, InternalNotificationRepository>();
            services.AddHttpClient();
            services.AddHttpContextAccessor();
        }
    }
}

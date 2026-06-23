using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.ExternalServices.EmailServices;
using Graduation_Application.IRepositories;
using Graduation_Application.IRepositories.Admin;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_Application.Mapper.Admin;
using Graduation_Application.Mapper.ProductMapping;
using Graduation_Application.Mapper.InspirationMapping;
using Graduation_Application.Options;
using Graduation_Application.Services;
using Graduation_Application.Services.Admin;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Graduation_infrastructure.Identity;
using Graduation_infrastructure.Localization;
using Graduation_infrastructure.Repositories;
using Graduation_infrastructure.Repositories.Admin;
using Graduation_infrastructure.Services;
using Graduation_infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_infrastructure.ProgramService.ServicesMVC
{
    public static class ServiceMVC
    {
        public static void AddInfrastructureMVC(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Database
            var connectionString = configuration.GetConnectionString("GraduationDbOnline");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString).EnableSensitiveDataLogging()
            );

            // Cookie Authentication — لازم يكون قبل Identity
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Account/Login";
                    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    //options.SlidingExpiration = true;
                    options.SlidingExpiration = false;

                });

            // Identity Core — من غير ما تـ override الـ Authentication
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddSignInManager();

            services.AddAuthorization();

            // Generic Repository
            services.AddScoped(typeof(IGenaricRepositories<>), typeof(GenaricRepositories<>));

            // Mapping
            ProductMappingConfig.RegisterMappings();
            AdminAuthMappingConfig.RegisterMappings();
            AdminProfileMappingConfig.RegisterMappings();
            AdminUsersMappingConfig.RegisterMappings();
            InspirationMappingConfig.RegisterMappings();

            // Admin Services
            services.AddScoped<IAdminProductService, AdminProductService>();
            services.AddScoped<IAdminReviewService, AdminReviewService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAdminVendorsRepository, AdminVendorsRepository>();
            services.AddScoped<IAdminVendorsService, AdminVendorsService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IAdminUsersService, AdminUsersService>();
            services.AddScoped<IAdminAuditLogsService, AdminAuditLogsService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<IOrderReviewImageRepository, OrderReviewImageRepository>();
            services.AddScoped<IAdminInspirationService, AdminInspirationService>();
            // Application Services
            services.AddScoped<IJwtTokenGenerator, NullJwtTokenGenerator>();
            // Do not register existing IAuthService here for MVC; register Admin auth service instead
            services.AddScoped<IAdminAuthService, AdminAuthService>();
            services.AddScoped<IAdminProfileService, AdminProfileService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();
            services.Configure<WhatsAppNotificationSettings>(
                configuration.GetSection("WhatsAppNotification")
            );
            services.AddSignalR();
            services.AddScoped<INotificationHub, NotificationHubService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IInternalNotificationService, InternalNotificationService>();
            services.AddScoped<IInternalNotificationRepository, InternalNotificationRepository>();

            services.AddHttpClient();
            services.AddHttpContextAccessor();

            services.Configure<PaymobSettings>(configuration.GetSection("Paymob"));
            services.AddScoped<IPaymentGateway, PaymobService>();
            services.AddScoped<IPaymobHmacValidator, PaymobHmacValidator>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        }
    }
}
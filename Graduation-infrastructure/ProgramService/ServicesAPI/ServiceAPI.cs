using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.ExternalServices.EmailServices;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_Application.Mapper.CategoryMapping;
using Graduation_Application.Mapper.NotificationMapping;
using Graduation_Application.Mapper.ProductMapping;
using Graduation_Application.Mapper.ReviewMapping;
using Graduation_Application.Mapper.UsersMapping;
using Graduation_Application.Mapper.VendorMapping;
using Graduation_Application.Mapper.InspirationMapping;
using Graduation_Application.Options;
using Graduation_Application.Services;
using Graduation_Application.Services.Admin;
using Graduation_Application.IServices.Vendor;
using Graduation_Application.Services.Vendor;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Graduation_Infrastructure.Identity;
using Graduation_infrastructure.Repositories;
using Graduation_infrastructure.Services;
using Graduation_infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
            });

            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return System.Threading.Tasks.Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return System.Threading.Tasks.Task.CompletedTask;
                };
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // Register Mapping Configurations
            RegisterMappingConfig.RegisterMappings();
            GooglePayloadMappingConfig.RegisterMappings();
            AuthResponseMappingConfig.Response();
            CategoryMappingConfig.RegisterMappings();
            ProductMappingConfig.RegisterMappings();
            ReviewMappingConfig.RegisterMappings();
            UserProfileMappingConfig.RegisterMappings();
            VendorMappingConfig.RegisterMappings();
            InternalNotificationMappingConfig.RegisterMappings();
            VendorAuthResponseMappingConfig.Response();
            VendorProfileMappingConfig.RegisterMappings();
            UpdateVendorProfileMappingConfig.RegisterMappings();
            InspirationMappingConfig.RegisterMappings();

            // Register Generic Repository
            services.AddMemoryCache();
            services.AddScoped(typeof(IGenaricRepositories<>), typeof(GenaricRepositories<>));

            // Register Services
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IVendorService, VendorService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISubCategoryService, SubCategoryService>();
            services.AddScoped<IProductTypeService, ProductTypeService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IVendorProductService, VendorProductService>();
            services.AddScoped<IVendorMaterialService, VendorMaterialService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IVendorOrderService, VendorOrderService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();
            services.Configure<WhatsAppNotificationSettings>(
                configuration.GetSection("WhatsAppNotification")
            );
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IInternalNotificationService, InternalNotificationService>();
            services.AddScoped<IInternalNotificationRepository, InternalNotificationRepository>();
            services.AddSignalR();
            services.AddScoped<INotificationHub, NotificationHubService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            services.AddScoped<IPaymentWebhookLogRepository, PaymentWebhookLogRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<ISpeechToTextService, ElevenLabsSpeechToTextService>();
            services.AddScoped<IVoiceChatService, VoiceChatService>();
            services.AddScoped<IAdminAuditLogsService, AdminAuditLogsService>();
            services.AddScoped<ILanguageUserService, LanguageUserService>();
            services.AddScoped<IOrderReviewImageRepository, OrderReviewImageRepository>();
            services.AddScoped<IInspirationService, InspirationService>();
            services.AddScoped<IAdminInspirationService, AdminInspirationService>();
            services.AddHttpClient();
            services.AddHttpClient("N8NChatClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(120); // Increased timeout to prevent timeouts during complex AI voice operations
            });
            services.AddHttpContextAccessor();
            // Add CORS policy for development / frontend
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAll",
                    builder => builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
            .AllowCredentials()
                );
            });

            var jwtSecret = configuration["Jwt:Secret"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtSecret)
                        ),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/notifications"))
                            {
                                context.Token = accessToken;
                            }
                            return System.Threading.Tasks.Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) 
                                         ?? context.Principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                            if (!string.IsNullOrEmpty(userId))
                            {
                                var user = await userManager.FindByIdAsync(userId);
                                if (user == null || !user.IsActive)
                                {
                                    context.Fail("Your account is currently inactive or suspended.");
                                }
                            }
                        }
                    };
                });

            services.Configure<PaymobSettings>(configuration.GetSection("Paymob"));
            services.Configure<N8NOptions>(configuration.GetSection("N8N"));
            services.Configure<ElevenLabsOptions>(configuration.GetSection("ElevenLabs"));
            services.AddScoped<IPaymentGateway, PaymobService>();
            services.AddScoped<IPaymobHmacValidator, PaymobHmacValidator>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

            services.AddAuthorization();
        }
    }
}

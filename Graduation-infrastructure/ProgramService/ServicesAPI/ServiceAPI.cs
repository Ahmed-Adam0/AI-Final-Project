using Graduation_Application.IServices;
using Graduation_Application.IRepositories;
using Graduation_Application.Mapper.UsersMapping;
using Graduation_Application.Mapper.CategoryMapping;
using Graduation_Application.Mapper.ProductMapping;
using Graduation_Application.Services;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Graduation_infrastructure.Repositories;
using Graduation_Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

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
#if DEBUG
                options.EnableSensitiveDataLogging();
#endif
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
            AuthResponseMappingConfig.Response();
            CategoryMappingConfig.RegisterMappings();
            ProductMappingConfig.RegisterMappings();

            // Register Generic Repository
            services.AddScoped(typeof(IGenaricRepositories<>), typeof(GenaricRepositories<>));

            // Register Services
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // Add CORS policy for development / frontend
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());
            });

            var jwtSecret = configuration["Jwt:Secret"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            services.AddAuthentication(options =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

            services.AddAuthorization();
        }
    }
}

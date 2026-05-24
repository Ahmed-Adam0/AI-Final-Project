using Graduation_Application.IServices;
using Graduation_Application.Mapper.UsersMapping;
using Graduation_Application.Services;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Graduation_Infrastructure.Identity;
using Graduation_infrastructure.Services;
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
                options.UseSqlServer(connectionString).EnableSensitiveDataLogging()
            );

            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            RegisterMappingConfig.RegisterMappings();
            AuthResponseMappingConfig.Response();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();

            var jwtSecret = configuration["Jwt:Secret"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            services.AddAuthentication("Bearer")
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
        }
    }
}

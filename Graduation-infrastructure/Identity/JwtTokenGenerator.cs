using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Graduation_Infrastructure.Identity
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public JwtTokenGenerator(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            var secret = _configuration["Jwt:Secret"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            //var expiryInMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "1440");
            var expiryInDays = int.Parse(_configuration["Jwt:ExpiryInDays"] ?? "1");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                // ensure NameIdentifier claim exists so controllers can read User.FindFirst(ClaimTypes.NameIdentifier)
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim("name", user.FullName ?? string.Empty),
                new Claim("lang", user.PreferredLanguage ?? string.Empty),
            };

            var workshop = _context.Workshops.FirstOrDefault(w => w.UserId == user.Id);
            if (workshop != null)
            {
                claims.Add(new Claim("WorkshopId", workshop.Id.ToString()));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryInDays),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

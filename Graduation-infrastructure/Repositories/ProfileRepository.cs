using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graduation_infrastructure.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfileRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApplicationUser?> GetWithAddressesAsync(string userId)
        {
            return await _dbContext.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdateProfileAsync(ApplicationUser user, List<Address>? newAddresses)
        {
            if (newAddresses != null)
            {
                _dbContext.Set<Address>()
                    .RemoveRange(user.Addresses ?? new List<Address>());

                await _dbContext.Set<Address>().AddRangeAsync(newAddresses);
            }

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string userName, string excludeUserId)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.UserName == userName && u.Id != excludeUserId);
        }

        public async Task<bool> EmailExistsAsync(string email, string excludeUserId)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.Email == email && u.Id != excludeUserId);
        }
    }
}
using Graduation_domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graduation_Application.IRepositories
{
    public interface IProfileRepository
    {
        Task<ApplicationUser?> GetWithAddressesAsync(string userId);
        Task<bool> UpdateProfileAsync(ApplicationUser user, List<Address>? newAddresses);
        Task<bool> UsernameExistsAsync(string userName, string excludeUserId);
        Task<bool> EmailExistsAsync(string email, string excludeUserId);
    }
}
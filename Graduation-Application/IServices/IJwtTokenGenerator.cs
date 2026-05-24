using System.Collections.Generic;
using Graduation_domain.Entities;

namespace Graduation_Application.IServices
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}

using Graduation_Application.IServices;
using Graduation_domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_infrastructure.Identity
{
    public class NullJwtTokenGenerator : IJwtTokenGenerator
    {
        public string GenerateToken(ApplicationUser user, IList<string> roles)
            => string.Empty;
    }
}

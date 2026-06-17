using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.DTOs.UserLanguageDTO
{
    public class UpdateUserLanguageCommand
    {
        public string UserId { get; set; }
        public string Language { get; set; }
    }
}

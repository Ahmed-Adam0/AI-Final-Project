using System;
using System.ComponentModel.DataAnnotations;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class ProposeDeliveryDateRequestDto
    {
        [Required(ErrorMessage = "Estimated delivery date is required")]
        [FutureDate(ErrorMessage = "Estimated delivery date must be in the future (current time or later). / يجب أن يكون تاريخ التوصيل المتوقع في المستقبل (الوقت الحالي أو بعده).")]
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}

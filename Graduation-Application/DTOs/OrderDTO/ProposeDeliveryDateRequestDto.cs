using System;
using System.ComponentModel.DataAnnotations;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class ProposeDeliveryDateRequestDto
    {
        [Required(ErrorMessage = "Estimated delivery date is required")]
        [FutureDate(ErrorMessage = "Estimated delivery date must be today or in the future. / يجب أن يكون تاريخ التوصيل المتوقع من اليوم فصاعداً.")]
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}

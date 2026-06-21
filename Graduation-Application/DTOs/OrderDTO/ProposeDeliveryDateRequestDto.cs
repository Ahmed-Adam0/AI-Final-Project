using System;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class ProposeDeliveryDateRequestDto
    {
        [Required(ErrorMessage = "Estimated delivery date is required")]
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}

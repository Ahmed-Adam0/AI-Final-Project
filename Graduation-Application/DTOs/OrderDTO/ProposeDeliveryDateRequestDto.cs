using System;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class ProposeDeliveryDateRequestDto
    {
        [LocalizedRequired(ErrorMessage = "validation.estimatedDeliveryDateRequired")]
        [FutureDate(ErrorMessage = "validation.estimatedDeliveryDateFuture")]
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}

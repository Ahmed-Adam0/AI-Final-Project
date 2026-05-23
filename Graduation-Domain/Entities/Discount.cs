using System;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Discount : BaseEntity<int>
    {
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount value is required")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
    }
}

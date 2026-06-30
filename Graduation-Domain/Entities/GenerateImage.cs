using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_domain.Entities
{
    public class GenerateImage : BaseEntity<int>
    {
        [Required]
        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }
        [Required]
        public string EmptyRoom { get; set; }

        [Column("generateImage")]
        public string GenerateImageUrl { get; set; }
        public int? OrderID { get; set; }

        [ForeignKey("OrderID")]
        public Order Order { get; set; }

        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
    }
}

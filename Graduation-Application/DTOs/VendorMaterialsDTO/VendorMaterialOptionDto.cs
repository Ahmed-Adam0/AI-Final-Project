namespace Graduation_Application.DTOs.VendorMaterialsDTO
{
    public class VendorMaterialOptionDto
    {
        public int Id { get; set; }
        public int VendorMaterialGroupId { get; set; }
        public string ValueAr { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; }
    }
}

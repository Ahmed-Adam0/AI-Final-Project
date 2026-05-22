namespace Graduation_infrastructure.Entities
{
    public class Address : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string CityAr { get; set; } = string.Empty;
        public string AreaAr { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}

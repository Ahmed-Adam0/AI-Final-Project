namespace Graduation_infrastructure.Entities
{
    public class Category : BaseEntity<int>
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
    }
}

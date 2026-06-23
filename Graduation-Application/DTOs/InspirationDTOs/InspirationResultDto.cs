using System.Collections.Generic;

namespace Graduation_Application.DTOs.InspirationDTOs
{
    public class InspirationResultDto
    {
        public List<InspirationItemDto> Data { get; set; } = [];
        public InspirationPaginationDto Pagination { get; set; }
    }

    public class InspirationItemDto
    {
        public string Id { get; set; }
        public string BeforeImageUrl { get; set; }
        public string AfterImageUrl { get; set; }
    }

    public class InspirationPaginationDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
    }
}

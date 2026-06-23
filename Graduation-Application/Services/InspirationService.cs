using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.InspirationDTOs;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class InspirationService : IInspirationService
    {
        private readonly IOrderReviewImageRepository _repository;
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IFileService _fileService;

        public InspirationService(
            IOrderReviewImageRepository repository,
            IGenaricRepositories<Order> orderRepository,
            IFileService fileService)
        {
            _repository = repository;
            _orderRepository = orderRepository;
            _fileService = fileService;
        }

        public async Task UploadInspirationsAsync(string userId, UploadInspirationsDto dto)
        {
            // 1. Validate order exists
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            // 2. Validate order belongs to authenticated customer
            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("This order does not belong to the authenticated customer.");
            }

            // 3. Validate matching images counts
            if (dto.BeforeImages == null || dto.AfterImages == null || dto.BeforeImages.Count != dto.AfterImages.Count)
            {
                throw new ArgumentException("The number of Before images must match the number of After images.");
            }

            if (dto.BeforeImages.Count == 0)
            {
                throw new ArgumentException("At least one Before/After image pair is required.");
            }

            // 4. Upload to Cloudinary and collect URLs
            var imagesToAdd = new List<OrderReviewImage>();
            for (int i = 0; i < dto.BeforeImages.Count; i++)
            {
                // Upload before image
                var beforeUrl = await _fileService.SaveImageAsync(dto.BeforeImages[i], "inspirations");
                // Upload after image
                var afterUrl = await _fileService.SaveImageAsync(dto.AfterImages[i], "inspirations");

                imagesToAdd.Add(new OrderReviewImage
                {
                    OrderId = dto.OrderId,
                    BeforeImageUrl = beforeUrl,
                    AfterImageUrl = afterUrl,
                    IsApproved = false
                });
            }

            // 5. Save to database
            await _repository.AddRangeAsync(imagesToAdd);
            await _repository.SaveChangesAsync();
        }

        public async Task<InspirationResultDto> GetApprovedInspirationsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 4;

            var query = _repository.Where(img => img.IsApproved && img.IsActive);
            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(img => img.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedData = items.Adapt<List<InspirationItemDto>>();

            return new InspirationResultDto
            {
                Data = mappedData,
                Pagination = new InspirationPaginationDto
                {
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    TotalCount = totalCount,
                    PageSize = pageSize
                }
            };
        }
    }
}

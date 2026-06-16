using Graduation_Application.DTOs.CategoryDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenaricRepositories<Category> _repository;

        public CategoryService(IGenaricRepositories<Category> repository)
        {
            _repository = repository;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _repository
                .GetAllAsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync();

            return categories.Adapt<List<CategoryDto>>();
        }

        public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync()
        {
            var categories = await _repository
                .GetAllAsNoTracking()
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.ProductTypes)
                .Where(c => c.IsActive)
                .ToListAsync();

            return categories.Adapt<List<CategoryTreeDto>>();
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            if (createCategoryDto == null)
                throw new ArgumentNullException(nameof(createCategoryDto));

            var category = new Category
            {
                NameAr = createCategoryDto.NameAr,
                NameEn = createCategoryDto.NameEn,
                ImageUrl = createCategoryDto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(category);
            await _repository.SaveChangesAsync();

            return category.Adapt<CategoryResponseDto>();
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateCategoryDto)
        {
            if (updateCategoryDto == null)
                throw new ArgumentNullException(nameof(updateCategoryDto));

            var category = await _repository.GetByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException($"Category with ID {categoryId} not found.");

            category.NameAr = updateCategoryDto.NameAr;
            category.NameEn = updateCategoryDto.NameEn;
            category.ImageUrl = updateCategoryDto.ImageUrl;
            category.UpdatedAt = DateTime.UtcNow;

            _repository.Update(category);
            await _repository.SaveChangesAsync();

            return category.Adapt<CategoryResponseDto>();
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category == null)
                return false;

            _repository.Delete(category);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}

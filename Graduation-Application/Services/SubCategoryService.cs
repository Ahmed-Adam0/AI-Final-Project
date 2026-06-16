using Graduation_Application.DTOs.CategoryDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Graduation_Application.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly IGenaricRepositories<SubCategory> _subCategoryRepository;
        private readonly IGenaricRepositories<Category> _categoryRepository;

        public SubCategoryService(
            IGenaricRepositories<SubCategory> subCategoryRepository,
            IGenaricRepositories<Category> categoryRepository)
        {
            _subCategoryRepository = subCategoryRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<List<SubCategoryDto>> GetAllSubCategoriesAsync()
        {
            var subCategories = await _subCategoryRepository
                .GetAllAsNoTracking()
                .Include(sc => sc.Category)
                .Where(sc => sc.IsActive)
                .ToListAsync();

            return subCategories.Adapt<List<SubCategoryDto>>();
        }

        public async Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(int categoryId)
        {
            var subCategories = await _subCategoryRepository
                .GetAllAsNoTracking()
                .Include(sc => sc.Category)
                .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
                .ToListAsync();

            return subCategories.Adapt<List<SubCategoryDto>>();
        }

        public async Task<SubCategoryDto> GetSubCategoryByIdAsync(int id)
        {
            var subCategory = await _subCategoryRepository
                .GetAllAsNoTracking()
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == id && sc.IsActive);

            if (subCategory == null)
                return null!;

            return subCategory.Adapt<SubCategoryDto>();
        }

        public async Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createSubCategoryDto)
        {
            if (createSubCategoryDto == null)
                throw new ArgumentNullException(nameof(createSubCategoryDto));

            var categoryExists = await _categoryRepository.AnyAsync(c => c.Id == createSubCategoryDto.CategoryId);
            if (!categoryExists)
                throw new ArgumentException($"Category with ID {createSubCategoryDto.CategoryId} does not exist.");

            var subCategory = new SubCategory
            {
                NameAr = createSubCategoryDto.NameAr,
                NameEn = createSubCategoryDto.NameEn,
                CategoryId = createSubCategoryDto.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _subCategoryRepository.AddAsync(subCategory);
            await _subCategoryRepository.SaveChangesAsync();

            // Fetch with Category navigation loaded for mapping Name
            var result = await _subCategoryRepository
                .GetAllAsNoTracking()
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == subCategory.Id);

            return result.Adapt<SubCategoryDto>();
        }

        public async Task<SubCategoryDto> UpdateSubCategoryAsync(int id, UpdateSubCategoryDto updateSubCategoryDto)
        {
            if (updateSubCategoryDto == null)
                throw new ArgumentNullException(nameof(updateSubCategoryDto));

            var subCategory = await _subCategoryRepository.GetByIdAsync(id);
            if (subCategory == null)
                throw new ArgumentException($"SubCategory with ID {id} not found.");

            var categoryExists = await _categoryRepository.AnyAsync(c => c.Id == updateSubCategoryDto.CategoryId);
            if (!categoryExists)
                throw new ArgumentException($"Category with ID {updateSubCategoryDto.CategoryId} does not exist.");

            subCategory.NameAr = updateSubCategoryDto.NameAr;
            subCategory.NameEn = updateSubCategoryDto.NameEn;
            subCategory.CategoryId = updateSubCategoryDto.CategoryId;
            subCategory.UpdatedAt = DateTime.UtcNow;

            _subCategoryRepository.Update(subCategory);
            await _subCategoryRepository.SaveChangesAsync();

            // Fetch with Category navigation loaded
            var result = await _subCategoryRepository
                .GetAllAsNoTracking()
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == subCategory.Id);

            return result.Adapt<SubCategoryDto>();
        }

        public async Task<bool> DeleteSubCategoryAsync(int id)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(id);
            if (subCategory == null)
                return false;

            _subCategoryRepository.Delete(subCategory);
            await _subCategoryRepository.SaveChangesAsync();

            return true;
        }
    }
}

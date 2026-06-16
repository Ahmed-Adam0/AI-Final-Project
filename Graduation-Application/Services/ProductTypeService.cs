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
    public class ProductTypeService : IProductTypeService
    {
        private readonly IGenaricRepositories<ProductType> _productTypeRepository;
        private readonly IGenaricRepositories<SubCategory> _subCategoryRepository;

        public ProductTypeService(
            IGenaricRepositories<ProductType> productTypeRepository,
            IGenaricRepositories<SubCategory> subCategoryRepository)
        {
            _productTypeRepository = productTypeRepository;
            _subCategoryRepository = subCategoryRepository;
        }

        public async Task<List<ProductTypeDto>> GetAllProductTypesAsync()
        {
            var productTypes = await _productTypeRepository
                .GetAllAsNoTracking()
                .Include(pt => pt.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Where(pt => pt.IsActive)
                .ToListAsync();

            return productTypes.Adapt<List<ProductTypeDto>>();
        }

        public async Task<List<ProductTypeDto>> GetProductTypesBySubCategoryIdAsync(int subCategoryId)
        {
            var productTypes = await _productTypeRepository
                .GetAllAsNoTracking()
                .Include(pt => pt.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Where(pt => pt.SubCategoryId == subCategoryId && pt.IsActive)
                .ToListAsync();

            return productTypes.Adapt<List<ProductTypeDto>>();
        }

        public async Task<ProductTypeDto> GetProductTypeByIdAsync(int id)
        {
            var productType = await _productTypeRepository
                .GetAllAsNoTracking()
                .Include(pt => pt.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.IsActive);

            if (productType == null)
                return null!;

            return productType.Adapt<ProductTypeDto>();
        }

        public async Task<ProductTypeDto> CreateProductTypeAsync(CreateProductTypeDto createProductTypeDto)
        {
            if (createProductTypeDto == null)
                throw new ArgumentNullException(nameof(createProductTypeDto));

            var subCategoryExists = await _subCategoryRepository.AnyAsync(sc => sc.Id == createProductTypeDto.SubCategoryId);
            if (!subCategoryExists)
                throw new ArgumentException($"SubCategory with ID {createProductTypeDto.SubCategoryId} does not exist.");

            var productType = new ProductType
            {
                NameAr = createProductTypeDto.NameAr,
                NameEn = createProductTypeDto.NameEn,
                SubCategoryId = createProductTypeDto.SubCategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productTypeRepository.AddAsync(productType);
            await _productTypeRepository.SaveChangesAsync();

            // Fetch with loaded hierarchy for mapping
            var result = await _productTypeRepository
                .GetAllAsNoTracking()
                .Include(pt => pt.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(pt => pt.Id == productType.Id);

            return result.Adapt<ProductTypeDto>();
        }

        public async Task<ProductTypeDto> UpdateProductTypeAsync(int id, UpdateProductTypeDto updateProductTypeDto)
        {
            if (updateProductTypeDto == null)
                throw new ArgumentNullException(nameof(updateProductTypeDto));

            var productType = await _productTypeRepository.GetByIdAsync(id);
            if (productType == null)
                throw new ArgumentException($"ProductType with ID {id} not found.");

            var subCategoryExists = await _subCategoryRepository.AnyAsync(sc => sc.Id == updateProductTypeDto.SubCategoryId);
            if (!subCategoryExists)
                throw new ArgumentException($"SubCategory with ID {updateProductTypeDto.SubCategoryId} does not exist.");

            productType.NameAr = updateProductTypeDto.NameAr;
            productType.NameEn = updateProductTypeDto.NameEn;
            productType.SubCategoryId = updateProductTypeDto.SubCategoryId;
            productType.UpdatedAt = DateTime.UtcNow;

            _productTypeRepository.Update(productType);
            await _productTypeRepository.SaveChangesAsync();

            // Fetch with loaded hierarchy
            var result = await _productTypeRepository
                .GetAllAsNoTracking()
                .Include(pt => pt.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(pt => pt.Id == productType.Id);

            return result.Adapt<ProductTypeDto>();
        }

        public async Task<bool> DeleteProductTypeAsync(int id)
        {
            var productType = await _productTypeRepository.GetByIdAsync(id);
            if (productType == null)
                return false;

            _productTypeRepository.Delete(productType);
            await _productTypeRepository.SaveChangesAsync();

            return true;
        }
    }
}

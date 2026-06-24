using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<Category> _categoryRepository;
        private readonly IGenaricRepositories<ProductImage> _productImageRepository;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly IGenaricRepositories<ProductAttribute> _attributeRepository;
        private readonly IGenaricRepositories<ProductAttributeValue> _attributeValueRepository;
        private readonly IGenaricRepositories<ProductMaterialOption> _productMaterialOptionRepository;
        private readonly IGenaricRepositories<ProductType> _productTypeRepository;
        private readonly IGenaricRepositories<Review> _reviewRepository;
        private readonly IFileService _fileService;

        public ProductService(
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<Category> categoryRepository,
            IGenaricRepositories<ProductImage> productImageRepository,
            IGenaricRepositories<Workshop> workshopRepository,
            IGenaricRepositories<ProductAttribute> attributeRepository,
            IGenaricRepositories<ProductAttributeValue> attributeValueRepository,
            IGenaricRepositories<ProductMaterialOption> productMaterialOptionRepository,
            IGenaricRepositories<ProductType> productTypeRepository,
            IGenaricRepositories<Review> reviewRepository,
            IFileService fileService
        )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
            _workshopRepository = workshopRepository;
            _attributeRepository = attributeRepository;
            _attributeValueRepository = attributeValueRepository;
            _productMaterialOptionRepository = productMaterialOptionRepository;
            _productTypeRepository = productTypeRepository;
            _reviewRepository = reviewRepository;
            _fileService = fileService;
        }

        public async Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            // Validate pagination parameters
            int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            // Start with base query (AsNoTracking for performance)
            IQueryable<Product> query = _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Images);

            // Apply IsActive status filter: default to showing only active products
            bool activeFilter = filter.IsActive ?? true;
            query = query.Where(p => p.IsActive == activeFilter && !p.IsHidden);

            // Apply Search Filter (Name and Description)
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string searchTerm = filter.Search.ToLower();
                query = query.Where(p =>
                    p.NameAr.ToLower().Contains(searchTerm)
                    || p.NameEn.ToLower().Contains(searchTerm)
                    || p.DescriptionAr.ToLower().Contains(searchTerm)
                    || p.DescriptionEn.ToLower().Contains(searchTerm)
                );
            }

            // Apply Category Filters (3-tier)
            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
            {
                query = query.Where(p =>
                    p.ProductType.SubCategory.CategoryId == filter.CategoryId.Value
                );
            }

            if (filter.SubCategoryId.HasValue && filter.SubCategoryId > 0)
            {
                query = query.Where(p => p.ProductType.SubCategoryId == filter.SubCategoryId.Value);
            }

            if (filter.ProductTypeId.HasValue && filter.ProductTypeId > 0)
            {
                query = query.Where(p => p.ProductTypeId == filter.ProductTypeId.Value);
            }

            // Apply Workshop Filter
            if (filter.WorkshopId.HasValue && filter.WorkshopId > 0)
            {
                query = query.Where(p => p.WorkshopId == filter.WorkshopId.Value);
            }

            // Apply Price Range Filter
            if (filter.MinPrice.HasValue && filter.MinPrice > 0)
            {
                query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue && filter.MaxPrice > 0)
            {
                query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);
            }

            // Apply Material Filter (search in description if not a separate field)
            if (!string.IsNullOrWhiteSpace(filter.Material))
            {
                string materialTerm = filter.Material.ToLower();
                query = query.Where(p =>
                    p.DescriptionAr.ToLower().Contains(materialTerm)
                    || p.DescriptionEn.ToLower().Contains(materialTerm)
                );
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                string sortTerm = filter.SortBy.ToLower().Trim();
                switch (sortTerm)
                {
                    case "price_asc":
                    case "priceasc":
                    case "price: low to high":
                    case "pricelowtohigh":
                    case "lowtohigh":
                        query = query.OrderBy(p => p.BasePrice);
                        break;
                    case "price_desc":
                    case "pricedesc":
                    case "price: high to low":
                    case "pricehightolow":
                    case "hightolow":
                        query = query.OrderByDescending(p => p.BasePrice);
                        break;
                    case "name_az":
                    case "nameaz":
                    case "name: a-z":
                    case "name":
                    case "az":
                        query = query.OrderBy(p => p.NameEn);
                        break;
                    case "rating":
                    case "ratingdesc":
                        query = query.OrderByDescending(p =>
                            _reviewRepository
                                .GetAllAsNoTracking()
                                .Where(r => r.ProductId == p.Id)
                                .Select(r => (double?)r.Rating)
                                .Average()
                            ?? 0
                        );
                        break;
                    case "newest":
                    case "newestdesc":
                    default:
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            // Get total count before pagination
            int totalCount = await query.CountAsync();

            // Apply Pagination (AFTER all filters, BEFORE execution)
            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
            var productDtos = products.Adapt<List<ProductDto>>();

            // Return paginated result
            return new PaginatedResult<ProductDto>(productDtos, totalCount, pageNumber, pageSize);
        }

        public async Task<ProductDetailsDto> GetProductDetailsAsync(int id)
        {
            var product = await _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Attributes)
                    .ThenInclude(a => a.Values)
                .Include(p => p.Images)
                .Include(p => p.MaterialOptions)
                    .ThenInclude(mo => mo.VendorMaterialOption)
                        .ThenInclude(o => o.Group)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive && !p.IsHidden);

            if (product == null)
                return null;

            return product.Adapt<ProductDetailsDto>();
        }

        public async Task<ProductDetailsDto> GetProductDetailsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
                
            string searchTerm = name.ToLower().Trim();

            var product = await _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Attributes)
                    .ThenInclude(a => a.Values)
                .Include(p => p.Images)
                .Include(p => p.MaterialOptions)
                    .ThenInclude(mo => mo.VendorMaterialOption)
                        .ThenInclude(o => o.Group)
                .FirstOrDefaultAsync(p => (p.NameAr.ToLower().Contains(searchTerm) || p.NameEn.ToLower().Contains(searchTerm)) && p.IsActive && !p.IsHidden);

            if (product == null)
                return null;

            return product.Adapt<ProductDetailsDto>();
        }

        public async Task<PaginatedResult<ProductEmbeddingDto>> GetProductsForEmbeddingAsync(
            int pageNumber,
            int pageSize
        )
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 100 : pageSize;

            IQueryable<Product> query = _productRepository.GetAllAsNoTracking();

            int totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductEmbeddingDto
                {
                    Text =
                        $"Id: {p.Id} | NameAr: {p.NameAr} | NameEn: {p.NameEn} | DescriptionAr: {p.DescriptionAr} | DescriptionEn: {p.DescriptionEn} | Price: {p.BasePrice} | CategoryNameAr: {p.ProductType.SubCategory.Category.NameAr} | CategoryNameEn: {p.ProductType.SubCategory.Category.NameEn} | SubCategoryNameAr: {p.ProductType.SubCategory.NameAr} | SubCategoryNameEn: {p.ProductType.SubCategory.NameEn} | VendorNameAr: {p.Workshop.WorkshopNameAr} | VendorNameEn: {p.Workshop.WorkshopNameEn}",
                })
                .ToListAsync();

            return new PaginatedResult<ProductEmbeddingDto>(
                products,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<string> GetProductEmbeddingTextAsync(int id)
        {
            var text = await _productRepository
                .GetAllAsNoTracking()
                .Where(p => p.Id == id)
                .Select(p =>
                    $"Id: {p.Id} | NameAr: {p.NameAr} | NameEn: {p.NameEn} | DescriptionAr: {p.DescriptionAr} | DescriptionEn: {p.DescriptionEn} | Price: {p.BasePrice} | CategoryNameAr: {p.ProductType.SubCategory.Category.NameAr} | CategoryNameEn: {p.ProductType.SubCategory.Category.NameEn} | SubCategoryNameAr: {p.ProductType.SubCategory.NameAr} | SubCategoryNameEn: {p.ProductType.SubCategory.NameEn} | VendorNameAr: {p.Workshop.WorkshopNameAr} | VendorNameEn: {p.Workshop.WorkshopNameEn}"
                )
                .FirstOrDefaultAsync();
            return text;
        }

        public async Task<ProductResponseDto> CreateProductAsync(
            string userId,
            CreateProductDto createProductDto
        )
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required.");

            if (createProductDto == null)
                throw new ArgumentNullException(nameof(createProductDto));

            var workshop = await _workshopRepository.FirstOrDefaultAsync(w => w.UserId == userId);
            if (workshop == null)
                throw new ArgumentException("Workshop not found for vendor.");

            var productTypeExists = await _productTypeRepository.AnyAsync(pt =>
                pt.Id == createProductDto.ProductTypeId
            );
            if (!productTypeExists)
                throw new ArgumentException(
                    $"ProductType with ID {createProductDto.ProductTypeId} does not exist."
                );

            // Create the base product (vendor-owned)
            var product = new Product
            {
                ProductTypeId = createProductDto.ProductTypeId,
                NameAr = createProductDto.NameAr,
                NameEn = createProductDto.NameEn,
                DescriptionAr = createProductDto.DescriptionAr,
                DescriptionEn = createProductDto.DescriptionEn,
                BasePrice = createProductDto.BasePrice,
                WorkshopId = workshop.Id,
                IsActive = true,
                IsHidden = false,
                CreatedAt = DateTime.UtcNow,
                MaterialOptions =
                    createProductDto
                        .MaterialOptions?.Select(mo => new ProductMaterialOption
                        {
                            VendorMaterialOptionId = mo.VendorMaterialOptionId,
                            PriceOption = mo.PriceOption,
                        })
                        .ToList()
                    ?? new List<ProductMaterialOption>(),
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            try
            {
                var text = await GetProductEmbeddingTextAsync(product.Id);
                if (!string.IsNullOrEmpty(text))
                {
                    using var client = new System.Net.Http.HttpClient();
                    string webhookUrl =
                        "https://main-production-aa56.up.railway.app/webhook/25fbe542-da87-4604-bfa6-ca1fa5f41f4e";

                    var jsonPayload = System.Text.Json.JsonSerializer.Serialize(
                        new { text = text }
                    );
                    var content = new System.Net.Http.StringContent(
                        jsonPayload,
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );
                    await client.PostAsync(webhookUrl, content);
                }
            }
            catch
            {
                // Ignore webhook failures so it doesn't break product creation
            }

            return product.Adapt<ProductResponseDto>();
        }

        public async Task<ProductResponseDto> UpdateProductAsync(
            int productId,
            string userId,
            UpdateProductDto updateProductDto
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            EnsureProductOwnership(product, userId);

            // Validate ProductType exists
            if (updateProductDto.ProductTypeId.HasValue)
            {
                var productTypeExists = await _productTypeRepository.AnyAsync(pt =>
                    pt.Id == updateProductDto.ProductTypeId.Value
                );
                if (!productTypeExists)
                    throw new ArgumentException(
                        $"ProductType with ID {updateProductDto.ProductTypeId.Value} does not exist."
                    );

                product.ProductTypeId = updateProductDto.ProductTypeId.Value;
            }

            product.NameAr = updateProductDto.NameAr;
            product.NameEn = updateProductDto.NameEn;
            product.DescriptionAr = updateProductDto.DescriptionAr;
            product.DescriptionEn = updateProductDto.DescriptionEn;
            product.UpdatedAt = DateTime.UtcNow;

            if (updateProductDto.BasePrice.HasValue)
            {
                product.BasePrice = updateProductDto.BasePrice.Value;
            }

            if (updateProductDto.IsActive.HasValue)
            {
                product.IsActive = updateProductDto.IsActive.Value;
            }

            if (updateProductDto.MaterialOptions != null)
            {
                var existingOptions = await _productMaterialOptionRepository
                    .Where(pmo => pmo.ProductId == productId)
                    .ToListAsync();
                _productMaterialOptionRepository.DeleteRange(existingOptions);

                var newOptions = updateProductDto.MaterialOptions.Select(
                    mo => new ProductMaterialOption
                    {
                        ProductId = productId,
                        VendorMaterialOptionId = mo.VendorMaterialOptionId,
                        PriceOption = mo.PriceOption,
                    }
                );
                await _productMaterialOptionRepository.AddRangeAsync(newOptions);
            }

            if (updateProductDto.Attributes != null)
            {
                var existingAttributes = await _attributeRepository
                    .Where(a => a.ProductId == productId)
                    .ToListAsync();
                _attributeRepository.DeleteRange(existingAttributes);

                var newAttributes = updateProductDto
                    .Attributes.Select(attr => new ProductAttribute
                    {
                        ProductId = productId,
                        NameAr = attr.NameAr,
                        NameEn = attr.NameEn,
                        Values =
                            attr.Values?.Select(val => new ProductAttributeValue
                                {
                                    ValueAr = val.ValueAr,
                                    ValueEn = val.ValueEn,
                                    PriceDelta = val.PriceDelta,
                                })
                                .ToList()
                            ?? new List<ProductAttributeValue>(),
                    })
                    .ToList();
                await _attributeRepository.AddRangeAsync(newAttributes);
            }

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            try
            {
                using var client = new System.Net.Http.HttpClient();

                // 1. Send delete request to remove the old embedding
                string deleteWebhookUrl =
                    "https://main-production-aa56.up.railway.app/webhook/fcaae4f1-d335-4338-827e-b5eb7dc0ee78";
                var deletePayload = System.Text.Json.JsonSerializer.Serialize(
                    new { id = product.Id }
                );
                var deleteContent = new System.Net.Http.StringContent(
                    deletePayload,
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
                await client.PostAsync(deleteWebhookUrl, deleteContent);

                // 2. Send add request to insert the new updated embedding
                var text = await GetProductEmbeddingTextAsync(product.Id);
                if (!string.IsNullOrEmpty(text))
                {
                    string addWebhookUrl =
                        "https://main-production-aa56.up.railway.app/webhook/25fbe542-da87-4604-bfa6-ca1fa5f41f4e";
                    var addPayload = System.Text.Json.JsonSerializer.Serialize(new { text = text });
                    var addContent = new System.Net.Http.StringContent(
                        addPayload,
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );
                    await client.PostAsync(addWebhookUrl, addContent);
                }
            }
            catch
            {
                // Ignore webhook failures so it doesn't break product update
            }

            return product.Adapt<ProductResponseDto>();
        }

        public async Task<bool> DeleteProductAsync(int productId, string userId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            EnsureProductOwnership(product, userId);

            if (!string.IsNullOrEmpty(product.Product3DModelUrl))
            {
                await _fileService.DeleteRawAsync(product.Product3DModelUrl);
            }

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();

            try
            {
                using var client = new System.Net.Http.HttpClient();
                string webhookUrl =
                    "https://main-production-aa56.up.railway.app/webhook/fcaae4f1-d335-4338-827e-b5eb7dc0ee78";

                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(new { id = productId });
                var content = new System.Net.Http.StringContent(
                    jsonPayload,
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
                await client.PostAsync(webhookUrl, content);
            }
            catch
            {
                // Ignore webhook failures so it doesn't break product deletion
            }

            return true;
        }

        public async Task<ProductImageDto> AddProductImageAsync(
            int productId,
            string userId,
            string imageUrl,
            bool isPrimary
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            EnsureProductOwnership(product, userId);

            // Check if this is the first image for this product
            var hasImages = await _productImageRepository.AnyAsync(pi => pi.ProductId == productId);
            if (!hasImages)
            {
                isPrimary = true;
            }
            else if (isPrimary)
            {
                // Set all other images to false
                var existingImages = await _productImageRepository
                    .Where(pi => pi.ProductId == productId)
                    .ToListAsync();
                foreach (var img in existingImages)
                {
                    if (img.IsPrimary)
                    {
                        img.IsPrimary = false;
                        _productImageRepository.Update(img);
                    }
                }
            }

            var newImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary,
                CreatedAt = DateTime.UtcNow,
            };

            await _productImageRepository.AddAsync(newImage);
            await _productImageRepository.SaveChangesAsync();

            return newImage.Adapt<ProductImageDto>();
        }

        public async Task<bool> RemoveProductImageAsync(int productId, string userId, int imageId)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
                return false;

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            EnsureProductOwnership(product, userId);

            bool wasPrimary = image.IsPrimary;

            _productImageRepository.Delete(image);
            await _productImageRepository.SaveChangesAsync();

            // If we deleted the primary image, set another remaining image as primary (if any)
            if (wasPrimary)
            {
                var remainingImages = await _productImageRepository
                    .Where(pi => pi.ProductId == productId)
                    .ToListAsync();
                var firstRemaining = remainingImages.FirstOrDefault();
                if (firstRemaining != null)
                {
                    firstRemaining.IsPrimary = true;
                    firstRemaining.UpdatedAt = DateTime.UtcNow;
                    _productImageRepository.Update(firstRemaining);
                    await _productImageRepository.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<ProductImageDto> ReplaceProductImageAsync(
            int productId,
            string userId,
            int imageId,
            string newImageUrl
        )
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
                throw new ArgumentException($"Image with ID {imageId} not found for this product.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            EnsureProductOwnership(product, userId);

            image.ImageUrl = newImageUrl;
            image.UpdatedAt = DateTime.UtcNow;

            _productImageRepository.Update(image);
            await _productImageRepository.SaveChangesAsync();

            return image.Adapt<ProductImageDto>();
        }

        public async Task<bool> SetPrimaryImageAsync(int productId, string userId, int imageId)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
                return false;

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            EnsureProductOwnership(product, userId);

            var otherImages = await _productImageRepository
                .Where(pi => pi.ProductId == productId && pi.Id != imageId)
                .ToListAsync();
            foreach (var img in otherImages)
            {
                if (img.IsPrimary)
                {
                    img.IsPrimary = false;
                    _productImageRepository.Update(img);
                }
            }

            image.IsPrimary = true;
            image.UpdatedAt = DateTime.UtcNow;

            _productImageRepository.Update(image);
            await _productImageRepository.SaveChangesAsync();

            return true;
        }

        public async Task<ProductResponseDto> SetProductStatusAsync(
            int productId,
            string userId,
            bool isActive
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            EnsureProductOwnership(product, userId);

            product.IsActive = isActive;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return product.Adapt<ProductResponseDto>();
        }

        public async Task<ProductAttributeDto> AddProductAttributeAsync(
            int productId,
            string userId,
            CreateProductAttributeDto dto
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Product not found.");
            EnsureProductOwnership(product, userId);

            var attribute = new ProductAttribute
            {
                ProductId = productId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                CreatedAt = DateTime.UtcNow,
            };
            await _attributeRepository.AddAsync(attribute);
            await _attributeRepository.SaveChangesAsync();

            return attribute.Adapt<ProductAttributeDto>();
        }

        public async Task<ProductAttributeDto> UpdateProductAttributeAsync(
            int productId,
            int attributeId,
            string userId,
            UpdateProductAttributeDto dto
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Product not found.");
            EnsureProductOwnership(product, userId);

            var attribute = await _attributeRepository.GetByIdAsync(attributeId);
            if (attribute == null || attribute.ProductId != productId)
                throw new ArgumentException("Attribute not found.");

            if (!string.IsNullOrWhiteSpace(dto.NameAr))
                attribute.NameAr = dto.NameAr;
            if (!string.IsNullOrWhiteSpace(dto.NameEn))
                attribute.NameEn = dto.NameEn;
            attribute.UpdatedAt = DateTime.UtcNow;

            _attributeRepository.Update(attribute);
            await _attributeRepository.SaveChangesAsync();

            return attribute.Adapt<ProductAttributeDto>();
        }

        public async Task<bool> DeleteProductAttributeAsync(
            int productId,
            int attributeId,
            string userId
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;
            EnsureProductOwnership(product, userId);

            var attribute = await _attributeRepository.GetByIdAsync(attributeId);
            if (attribute == null || attribute.ProductId != productId)
                return false;

            _attributeRepository.Delete(attribute);
            await _attributeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<AttributeValueDto> AddAttributeValueAsync(
            int productId,
            int attributeId,
            string userId,
            CreateProductAttributeValueDto dto
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Product not found.");
            EnsureProductOwnership(product, userId);

            var attribute = await _attributeRepository.GetByIdAsync(attributeId);
            if (attribute == null || attribute.ProductId != productId)
                throw new ArgumentException("Attribute not found.");

            var value = new ProductAttributeValue
            {
                AttributeId = attributeId,
                ValueAr = dto.ValueAr,
                ValueEn = dto.ValueEn,
                PriceDelta = dto.PriceDelta,
                CreatedAt = DateTime.UtcNow,
            };

            await _attributeValueRepository.AddAsync(value);
            await _attributeValueRepository.SaveChangesAsync();

            return value.Adapt<AttributeValueDto>();
        }

        public async Task<AttributeValueDto> UpdateAttributeValueAsync(
            int productId,
            int attributeId,
            int valueId,
            string userId,
            UpdateProductAttributeValueDto dto
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Product not found.");
            EnsureProductOwnership(product, userId);

            var value = await _attributeValueRepository.GetByIdAsync(valueId);
            if (value == null || value.AttributeId != attributeId)
                throw new ArgumentException("Value not found.");

            if (!string.IsNullOrWhiteSpace(dto.ValueAr))
                value.ValueAr = dto.ValueAr;
            if (!string.IsNullOrWhiteSpace(dto.ValueEn))
                value.ValueEn = dto.ValueEn;
            if (dto.PriceDelta.HasValue)
                value.PriceDelta = dto.PriceDelta.Value;
            value.UpdatedAt = DateTime.UtcNow;

            _attributeValueRepository.Update(value);
            await _attributeValueRepository.SaveChangesAsync();

            return value.Adapt<AttributeValueDto>();
        }

        public async Task<bool> DeleteAttributeValueAsync(
            int productId,
            int attributeId,
            int valueId,
            string userId
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;
            EnsureProductOwnership(product, userId);

            var value = await _attributeValueRepository.GetByIdAsync(valueId);
            if (value == null || value.AttributeId != attributeId)
                return false;

            _attributeValueRepository.Delete(value);
            await _attributeValueRepository.SaveChangesAsync();
            return true;
        }

        private static void EnsureProductOwnership(Product product, string userId)
        {
            var isOwner = product.Workshop?.UserId == userId;
            if (!isOwner && product.Workshop != null)
                throw new UnauthorizedAccessException(
                    "You do not have permission to manage this product."
                );
        }

        public async Task<string> UploadProduct3DModelAsync(int productId, string userId, Microsoft.AspNetCore.Http.IFormFile file)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            EnsureProductOwnership(product, userId);

            var folderName = "products/3d-models";
            var modelUrl = await _fileService.Save3DModelAsync(file, folderName, product.Product3DModelUrl);

            product.Product3DModelUrl = modelUrl;
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return modelUrl;
        }

        public async Task<bool> RemoveProduct3DModelAsync(int productId, string userId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            EnsureProductOwnership(product, userId);

            if (!string.IsNullOrEmpty(product.Product3DModelUrl))
            {
                await _fileService.DeleteRawAsync(product.Product3DModelUrl);
                product.Product3DModelUrl = null;
                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();
            }

            return true;
        }
    }
}

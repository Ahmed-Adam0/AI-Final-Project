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
    }
}

using Graduation_Application.DTOs.VendorMaterialsDTO;
using Graduation_Application.IServices.Vendor;
using Graduation_domain.Entities;
using Graduation_Application.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Graduation_Application.Services.Vendor
{
    public class VendorMaterialService : IVendorMaterialService
    {
        private readonly IGenaricRepositories<VendorMaterialGroup> _vendorMaterialGroupRepository;
        private readonly IGenaricRepositories<VendorMaterialOption> _vendorMaterialOptionRepository;
        private readonly IGenaricRepositories<ProductMaterialOption> _productMaterialOptionRepository;

        public VendorMaterialService(
            IGenaricRepositories<VendorMaterialGroup> vendorMaterialGroupRepository,
            IGenaricRepositories<VendorMaterialOption> vendorMaterialOptionRepository,
            IGenaricRepositories<ProductMaterialOption> productMaterialOptionRepository)
        {
            _vendorMaterialGroupRepository = vendorMaterialGroupRepository;
            _vendorMaterialOptionRepository = vendorMaterialOptionRepository;
            _productMaterialOptionRepository = productMaterialOptionRepository;
        }

        public async Task<VendorMaterialGroupDto> CreateGroupAsync(int workshopId, CreateVendorMaterialGroupDto dto)
        {
            var group = new VendorMaterialGroup
            {
                WorkshopId = workshopId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn
            };

            await _vendorMaterialGroupRepository.AddAsync(group);
            await _vendorMaterialGroupRepository.SaveChangesAsync();

            return new VendorMaterialGroupDto
            {
                Id = group.Id,
                WorkshopId = group.WorkshopId,
                NameAr = group.NameAr,
                NameEn = group.NameEn,
                Options = new List<VendorMaterialOptionDto>()
            };
        }

        public async Task<VendorMaterialOptionDto> AddOptionAsync(int workshopId, int groupId, CreateVendorMaterialOptionDto dto)
        {
            var group = await _vendorMaterialGroupRepository
                .Where(g => g.Id == groupId && g.WorkshopId == workshopId)
                .FirstOrDefaultAsync();

            if (group == null)
            {
                throw new Exception("Material group not found or you do not have permission.");
            }

            var option = new VendorMaterialOption
            {
                VendorMaterialGroupId = groupId,
                ValueAr = dto.ValueAr,
                ValueEn = dto.ValueEn,
                PriceDelta = dto.PriceDelta
            };

            await _vendorMaterialOptionRepository.AddAsync(option);
            await _vendorMaterialOptionRepository.SaveChangesAsync();

            return new VendorMaterialOptionDto
            {
                Id = option.Id,
                VendorMaterialGroupId = option.VendorMaterialGroupId,
                ValueAr = option.ValueAr,
                ValueEn = option.ValueEn,
                PriceDelta = option.PriceDelta
            };
        }

        public async Task<VendorMaterialOptionDto> UpdateOptionAsync(int workshopId, int optionId, UpdateVendorMaterialOptionDto dto)
        {
            var option = await _vendorMaterialOptionRepository
                .Where(o => o.Id == optionId && o.Group.WorkshopId == workshopId)
                .Include(o => o.Group)
                .FirstOrDefaultAsync();

            if (option == null)
            {
                throw new Exception("Material option not found or you do not have permission.");
            }

            // Calculate percentage and update related ProductMaterialOption prices
            if (option.PriceDelta != dto.PriceDelta)
            {
                if (option.PriceDelta == 0)
                {
                    throw new Exception("Cannot update percentage for an option with a base price of 0. Please update related products manually or provide a valid base price initially.");
                }

                decimal percentageChange = (dto.PriceDelta - option.PriceDelta) / option.PriceDelta;

                var relatedOptions = await _productMaterialOptionRepository
                    .Where(pmo => pmo.VendorMaterialOptionId == optionId)
                    .ToListAsync();

                foreach (var relatedOption in relatedOptions)
                {
                    relatedOption.PriceOption += (relatedOption.PriceOption * percentageChange);
                    _productMaterialOptionRepository.Update(relatedOption);
                }
                
                await _productMaterialOptionRepository.SaveChangesAsync();
            }

            option.ValueAr = dto.ValueAr;
            option.ValueEn = dto.ValueEn;
            option.PriceDelta = dto.PriceDelta;

            _vendorMaterialOptionRepository.Update(option);
            await _vendorMaterialOptionRepository.SaveChangesAsync();

            return new VendorMaterialOptionDto
            {
                Id = option.Id,
                VendorMaterialGroupId = option.VendorMaterialGroupId,
                ValueAr = option.ValueAr,
                ValueEn = option.ValueEn,
                PriceDelta = option.PriceDelta
            };
        }

        public async Task<List<VendorMaterialGroupDto>> GetVendorMaterialsAsync(int workshopId)
        {
            var groups = await _vendorMaterialGroupRepository
                .Where(g => g.WorkshopId == workshopId)
                .Include(g => g.Options)
                .ToListAsync();

            return groups.Select(g => new VendorMaterialGroupDto
            {
                Id = g.Id,
                WorkshopId = g.WorkshopId,
                NameAr = g.NameAr,
                NameEn = g.NameEn,
                Options = g.Options.Select(o => new VendorMaterialOptionDto
                {
                    Id = o.Id,
                    VendorMaterialGroupId = o.VendorMaterialGroupId,
                    ValueAr = o.ValueAr,
                    ValueEn = o.ValueEn,
                    PriceDelta = o.PriceDelta
                }).ToList()
            }).ToList();
        }

        public async Task DeleteGroupAsync(int workshopId, int groupId)
        {
            var group = await _vendorMaterialGroupRepository
                .Where(g => g.Id == groupId && g.WorkshopId == workshopId)
                .FirstOrDefaultAsync();

            if (group != null)
            {
                _vendorMaterialGroupRepository.Delete(group);
                await _vendorMaterialGroupRepository.SaveChangesAsync();
            }
        }

        public async Task DeleteOptionAsync(int workshopId, int optionId)
        {
            var option = await _vendorMaterialOptionRepository
                .Where(o => o.Id == optionId && o.Group.WorkshopId == workshopId)
                .Include(o => o.Group)
                .FirstOrDefaultAsync();

            if (option != null)
            {
                _vendorMaterialOptionRepository.Delete(option);
                await _vendorMaterialOptionRepository.SaveChangesAsync();
            }
        }
    }
}

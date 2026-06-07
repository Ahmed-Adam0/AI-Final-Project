using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.FaqDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class FaqService : IFaqService
    {
        private readonly IGenaricRepositories<Faq> _repository;

        public FaqService(IGenaricRepositories<Faq> repository)
        {
            _repository = repository;
        }

        public async Task<List<FaqDto>> GetAllFaqsAsync()
        {
            var faqs = await _repository
                .GetAllAsNoTracking()
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            return faqs.Adapt<List<FaqDto>>();
        }

        public async Task<FaqDto?> GetFaqByIdAsync(int id)
        {
            var faq = await _repository.GetByIdAsync(id);
            return faq?.Adapt<FaqDto>();
        }

        public async Task<FaqResponseDto> CreateFaqAsync(CreateFaqDto createFaqDto)
        {
            if (createFaqDto == null)
                throw new ArgumentNullException(nameof(createFaqDto));

            var faq = new Faq
            {
                QuestionAr = createFaqDto.QuestionAr,
                QuestionEn = createFaqDto.QuestionEn,
                AnswerAr = createFaqDto.AnswerAr,
                AnswerEn = createFaqDto.AnswerEn,
                DisplayOrder = createFaqDto.DisplayOrder,
                IsActive = createFaqDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(faq);
            await _repository.SaveChangesAsync();

            return faq.Adapt<FaqResponseDto>();
        }

        public async Task<FaqResponseDto> UpdateFaqAsync(int faqId, UpdateFaqDto updateFaqDto)
        {
            if (updateFaqDto == null)
                throw new ArgumentNullException(nameof(updateFaqDto));

            var faq = await _repository.GetByIdAsync(faqId);
            if (faq == null)
                throw new ArgumentException($"Faq with ID {faqId} not found.");

            faq.QuestionAr = updateFaqDto.QuestionAr;
            faq.QuestionEn = updateFaqDto.QuestionEn;
            faq.AnswerAr = updateFaqDto.AnswerAr;
            faq.AnswerEn = updateFaqDto.AnswerEn;
            faq.DisplayOrder = updateFaqDto.DisplayOrder;
            faq.IsActive = updateFaqDto.IsActive;
            faq.UpdatedAt = DateTime.UtcNow;

            _repository.Update(faq);
            await _repository.SaveChangesAsync();

            return faq.Adapt<FaqResponseDto>();
        }

        public async Task<bool> DeleteFaqAsync(int faqId)
        {
            var faq = await _repository.GetByIdAsync(faqId);
            if (faq == null)
                return false;

            _repository.Delete(faq);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateFaqStatusAsync(int faqId, bool isActive)
        {
            var faq = await _repository.GetByIdAsync(faqId);
            if (faq == null)
                return false;

            faq.IsActive = isActive;
            faq.UpdatedAt = DateTime.UtcNow;

            _repository.Update(faq);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}

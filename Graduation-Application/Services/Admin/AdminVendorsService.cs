using System;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.VendorManagementDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IRepositories.Admin;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Enums;
using Graduation_domain.Entities;

namespace Graduation_Application.Services.Admin
{
    public class AdminVendorsService : IAdminVendorsService
    {
        private readonly IAdminVendorsRepository _repository;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly IGenaricRepositories<ApplicationUser> _userRepository;
        private readonly IGenaricRepositories<VendorVerificationHistory> _verificationHistoryRepository;
        private readonly IGenaricRepositories<VendorAccountStatusHistory> _accountHistoryRepository;
        private readonly IInternalNotificationService _internalNotificationService;

        public AdminVendorsService(
            IAdminVendorsRepository repository,
            IGenaricRepositories<Workshop> workshopRepository,
            IGenaricRepositories<ApplicationUser> userRepository,
            IGenaricRepositories<VendorVerificationHistory> verificationHistoryRepository,
            IGenaricRepositories<VendorAccountStatusHistory> accountHistoryRepository,
            IInternalNotificationService internalNotificationService)
        {
            _repository = repository;
            _workshopRepository = workshopRepository;
            _userRepository = userRepository;
            _verificationHistoryRepository = verificationHistoryRepository;
            _accountHistoryRepository = accountHistoryRepository;
            _internalNotificationService = internalNotificationService;
        }

        public Task<AdminVendorsPageDto> GetVendorsPageAsync(AdminVendorsFilterDto filter, bool pendingOnly = false)
            => _repository.GetVendorsPageAsync(filter, pendingOnly);

        public Task<AdminVendorDetailsDto?> GetVendorDetailsAsync(int workshopId)
            => _repository.GetVendorDetailsAsync(workshopId);

        public Task<AdminVendorHistoryPageDto> GetHistoryAsync(int page, int pageSize)
            => _repository.GetHistoryAsync(page, pageSize);

        public async Task ApproveVendorAsync(int workshopId, string performedByAdminId, string? notes = null)
        {
            var workshop = await _workshopRepository.GetByIdAsync(workshopId);
            if (workshop == null)
                throw new ArgumentException("Vendor not found.");

            var oldStatus = workshop.VerificationStatus == 0 && workshop.IsVerified
                ? VendorVerificationStatus.Approved
                : workshop.VerificationStatus;

            workshop.VerificationStatus = VendorVerificationStatus.Approved;
            workshop.IsVerified = true;
            workshop.VerificationDate = DateTime.UtcNow;
            workshop.VerifiedByAdminId = performedByAdminId;
            workshop.VerificationNotes = notes;
            workshop.RejectionReason = null;
            workshop.UpdatedAt = DateTime.UtcNow;
            workshop.UpdatedBy = performedByAdminId;

            _workshopRepository.Update(workshop);

            await _verificationHistoryRepository.AddAsync(new VendorVerificationHistory
            {
                WorkshopId = workshopId,
                OldStatus = oldStatus,
                NewStatus = VendorVerificationStatus.Approved,
                Notes = notes,
                PerformedByAdminId = performedByAdminId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedByAdminId,
            });

            await _workshopRepository.SaveChangesAsync();
            await _verificationHistoryRepository.SaveChangesAsync();

            await _internalNotificationService.CreateAsync(workshop.UserId, NotificationType.AccountApproved);
        }

        public async Task RejectVendorAsync(int workshopId, string performedByAdminId, string rejectionReason, string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new ArgumentException("Rejection reason is required.");

            var workshop = await _workshopRepository.GetByIdAsync(workshopId);
            if (workshop == null)
                throw new ArgumentException("Vendor not found.");

            var oldStatus = workshop.VerificationStatus == 0 && workshop.IsVerified
                ? VendorVerificationStatus.Approved
                : workshop.VerificationStatus;

            workshop.VerificationStatus = VendorVerificationStatus.Rejected;
            workshop.IsVerified = false;
            workshop.VerificationDate = DateTime.UtcNow;
            workshop.VerifiedByAdminId = performedByAdminId;
            workshop.VerificationNotes = notes;
            workshop.RejectionReason = rejectionReason;
            workshop.UpdatedAt = DateTime.UtcNow;
            workshop.UpdatedBy = performedByAdminId;

            _workshopRepository.Update(workshop);

            await _verificationHistoryRepository.AddAsync(new VendorVerificationHistory
            {
                WorkshopId = workshopId,
                OldStatus = oldStatus,
                NewStatus = VendorVerificationStatus.Rejected,
                Notes = notes,
                RejectionReason = rejectionReason,
                PerformedByAdminId = performedByAdminId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedByAdminId,
            });

            await _workshopRepository.SaveChangesAsync();
            await _verificationHistoryRepository.SaveChangesAsync();

            await _internalNotificationService.CreateAsync(workshop.UserId, NotificationType.VendorAccountRejected);
        }

        public async Task ActivateVendorAsync(int workshopId, string performedByAdminId, string? notes = null)
        {
            var workshop = await _workshopRepository.GetByIdAsync(workshopId);
            if (workshop == null)
                throw new ArgumentException("Vendor not found.");

            var old = workshop.AccountStatus;
            if (old == VendorAccountStatus.Active)
                return;

            workshop.AccountStatus = VendorAccountStatus.Active;
            workshop.IsActive = true;
            workshop.AccountStatusChangedAt = DateTime.UtcNow;
            workshop.AccountStatusChangedByAdminId = performedByAdminId;
            workshop.UpdatedAt = DateTime.UtcNow;
            workshop.UpdatedBy = performedByAdminId;

            _workshopRepository.Update(workshop);

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == workshop.UserId);
            if (user != null)
            {
                user.IsActive = true;
                _userRepository.Update(user);
            }

            await _accountHistoryRepository.AddAsync(new VendorAccountStatusHistory
            {
                WorkshopId = workshopId,
                OldStatus = old,
                NewStatus = VendorAccountStatus.Active,
                Notes = notes,
                PerformedByAdminId = performedByAdminId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedByAdminId,
            });

            await _workshopRepository.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();
            await _accountHistoryRepository.SaveChangesAsync();

            await _internalNotificationService.CreateAsync(workshop.UserId, NotificationType.VendorAccountReactivated);
        }

        public async Task SuspendVendorAsync(int workshopId, string performedByAdminId, string reason, string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Suspension reason is required.");

            var workshop = await _workshopRepository.GetByIdAsync(workshopId);
            if (workshop == null)
                throw new ArgumentException("Vendor not found.");

            var old = workshop.AccountStatus;
            if (old == VendorAccountStatus.Suspended)
                return;

            workshop.AccountStatus = VendorAccountStatus.Suspended;
            workshop.IsActive = false;
            workshop.AccountStatusChangedAt = DateTime.UtcNow;
            workshop.AccountStatusChangedByAdminId = performedByAdminId;
            workshop.UpdatedAt = DateTime.UtcNow;
            workshop.UpdatedBy = performedByAdminId;

            _workshopRepository.Update(workshop);

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == workshop.UserId);
            if (user != null)
            {
                user.IsActive = false;
                _userRepository.Update(user);
            }

            await _accountHistoryRepository.AddAsync(new VendorAccountStatusHistory
            {
                WorkshopId = workshopId,
                OldStatus = old,
                NewStatus = VendorAccountStatus.Suspended,
                Reason = reason,
                Notes = notes,
                PerformedByAdminId = performedByAdminId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedByAdminId,
            });

            await _workshopRepository.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();
            await _accountHistoryRepository.SaveChangesAsync();

            await _internalNotificationService.CreateAsync(workshop.UserId, NotificationType.VendorAccountSuspended);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.WalletDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Graduation_Application.Services.Admin
{
    public class AdminWithdrawalsService : IAdminWithdrawalsService
    {
        private readonly IGenaricRepositories<VendorWithdrawal> _withdrawalRepository;
        private readonly IVendorWalletRepository _walletRepository;
        private readonly IPayoutGateway _payoutGateway;
        private readonly ILogger<AdminWithdrawalsService> _logger;

        public AdminWithdrawalsService(
            IGenaricRepositories<VendorWithdrawal> withdrawalRepository,
            IVendorWalletRepository walletRepository,
            IPayoutGateway payoutGateway,
            ILogger<AdminWithdrawalsService> logger)
        {
            _withdrawalRepository = withdrawalRepository;
            _walletRepository     = walletRepository;
            _payoutGateway        = payoutGateway;
            _logger               = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AdminWithdrawalDto>> GetAllWithdrawalsAsync()
        {
            var withdrawals = await _withdrawalRepository
                .GetAllAsNoTracking()
                .Include(w => w.Workshop)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return withdrawals.Select(w => new AdminWithdrawalDto
            {
                Id                   = w.Id,
                WorkshopId           = w.WorkshopId,
                WorkshopNameEn       = w.Workshop?.WorkshopNameEn ?? "Unknown Workshop",
                WorkshopNameAr       = w.Workshop?.WorkshopNameAr ?? "ورشة غير معروفة",
                Amount               = w.Amount,
                WalletNumber         = w.WalletNumber,
                Status               = w.Status.ToString(),
                TransactionReference = w.TransactionReference,
                CreatedAt            = w.CreatedAt
            });
        }

        /// <inheritdoc/>
        public async Task<bool> CompleteWithdrawalAsync(int withdrawalId)
        {
            // ── 1. Load withdrawal and wallet ──────────────────────────────────
            var withdrawal = await _withdrawalRepository
                .Where(w => w.Id == withdrawalId)
                .FirstOrDefaultAsync();

            if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending)
            {
                _logger.LogWarning("CompleteWithdrawalAsync: Withdrawal {Id} not found or not Pending.", withdrawalId);
                return false;
            }

            var wallet = await _walletRepository.GetByWorkshopIdAsync(withdrawal.WorkshopId);
            if (wallet == null)
            {
                _logger.LogWarning("CompleteWithdrawalAsync: Wallet not found for Workshop {Id}.", withdrawal.WorkshopId);
                return false;
            }

            if (withdrawal.Amount > wallet.AvailableBalance)
            {
                _logger.LogWarning(
                    "CompleteWithdrawalAsync: Insufficient wallet balance for Workshop {Id}. Required: {Required}, Available: {Available}",
                    withdrawal.WorkshopId, withdrawal.Amount, wallet.AvailableBalance);
                
                // If insufficient balance, reject the request to release the block
                withdrawal.Status = WithdrawalStatus.Failed;
                await _withdrawalRepository.SaveChangesAsync();
                return false;
            }

            // ── 2. Run actual/mock payout gateway ──────────────────────────────
            try
            {
                var transactionRef = await _payoutGateway.SendPayoutAsync(withdrawal.Amount, withdrawal.WalletNumber);

                // ── 3. Apply state and deduct balance ──────────────────────────────
                withdrawal.Status               = WithdrawalStatus.Completed;
                withdrawal.TransactionReference  = transactionRef;
                withdrawal.UpdatedAt             = DateTime.UtcNow;

                wallet.AvailableBalance -= withdrawal.Amount;
                wallet.TotalWithdrawn   += withdrawal.Amount;
                wallet.UpdatedAt         = DateTime.UtcNow;

                await _withdrawalRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "CompleteWithdrawalAsync: Payout success. PayoutId={Id}, WorkshopId={WId}, NetDeducted={Amount}, Ref={Ref}",
                    withdrawal.Id, withdrawal.WorkshopId, withdrawal.Amount, transactionRef);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CompleteWithdrawalAsync: Payout gateway execution failed. PayoutId={Id}", withdrawal.Id);
                
                withdrawal.Status    = WithdrawalStatus.Failed;
                withdrawal.UpdatedAt = DateTime.UtcNow;
                await _withdrawalRepository.SaveChangesAsync();

                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RejectWithdrawalAsync(int withdrawalId)
        {
            var withdrawal = await _withdrawalRepository
                .Where(w => w.Id == withdrawalId)
                .FirstOrDefaultAsync();

            if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending)
            {
                _logger.LogWarning("RejectWithdrawalAsync: Withdrawal {Id} not found or not Pending.", withdrawalId);
                return false;
            }

            withdrawal.Status    = WithdrawalStatus.Failed;
            withdrawal.UpdatedAt = DateTime.UtcNow;

            await _withdrawalRepository.SaveChangesAsync();

            _logger.LogInformation("RejectWithdrawalAsync: Payout rejected. PayoutId={Id}, WorkshopId={WId}, Amount={Amount}",
                withdrawal.Id, withdrawal.WorkshopId, withdrawal.Amount);

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.WalletDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Graduation_Application.Services.Vendor
{
    public class VendorWalletService : IVendorWalletService
    {
        private readonly IVendorWalletRepository _walletRepository;
        private readonly IGenaricRepositories<VendorWithdrawal> _withdrawalRepository;
        private readonly IPayoutGateway _payoutGateway;
        private readonly ILogger<VendorWalletService> _logger;

        public VendorWalletService(
            IVendorWalletRepository walletRepository,
            IGenaricRepositories<VendorWithdrawal> withdrawalRepository,
            IPayoutGateway payoutGateway,
            ILogger<VendorWalletService> logger)
        {
            _walletRepository     = walletRepository;
            _withdrawalRepository = withdrawalRepository;
            _payoutGateway        = payoutGateway;
            _logger               = logger;
        }

        /// <inheritdoc/>
        public async Task<VendorWalletDto> GetWalletAsync(int workshopId)
        {
            var wallet = await _walletRepository.GetByWorkshopIdAsync(workshopId);

            // Return zeroed wallet if it hasn't been created yet (no payments received).
            return wallet == null
                ? new VendorWalletDto { AvailableBalance = 0m, TotalWithdrawn = 0m }
                : new VendorWalletDto
                {
                    AvailableBalance = wallet.AvailableBalance,
                    TotalWithdrawn   = wallet.TotalWithdrawn
                };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VendorWithdrawalDto>> GetWithdrawalsAsync(int workshopId)
        {
            var withdrawals = await _withdrawalRepository
                .Where(w => w.WorkshopId == workshopId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return withdrawals.Select(MapWithdrawal);
        }

        /// <inheritdoc/>
        public async Task<VendorWithdrawalResultDto> RequestWithdrawalAsync(
            int workshopId,
            RequestWithdrawalDto dto)
        {
            // ── 1. Load wallet ──────────────────────────────────────────────────
            var wallet = await _walletRepository.GetByWorkshopIdAsync(workshopId);
            if (wallet == null || wallet.AvailableBalance <= 0)
                return Fail("No available balance to withdraw.");

            if (dto.Amount <= 0)
                return Fail("Withdrawal amount must be greater than zero.");

            // ── 2. Calculate effective balance after pending requests ──────────
            var pendingSum = await _withdrawalRepository
                .Where(w => w.WorkshopId == workshopId && w.Status == WithdrawalStatus.Pending)
                .SumAsync(w => w.Amount);

            var effectiveBalance = wallet.AvailableBalance - pendingSum;

            if (dto.Amount > effectiveBalance)
            {
                return Fail(effectiveBalance > 0
                    ? $"Insufficient balance. You have pending withdrawals of {pendingSum:N2} EGP. Available: {effectiveBalance:N2} EGP."
                    : $"Insufficient balance. You have pending withdrawals of {pendingSum:N2} EGP. Available: 0.00 EGP.");
            }

            // ── 3. Create withdrawal record (Pending) ──────────────────────────
            var withdrawal = new VendorWithdrawal
            {
                WorkshopId   = workshopId,
                Amount        = dto.Amount,
                WalletNumber  = dto.WalletNumber,
                Status        = WithdrawalStatus.Pending,
                CreatedAt     = DateTime.UtcNow
            };

            await _withdrawalRepository.AddAsync(withdrawal);
            await _withdrawalRepository.SaveChangesAsync();

            _logger.LogInformation(
                "VendorWithdrawal request submitted: Id={Id}, WorkshopId={WorkshopId}, Amount={Amount}, Status=Pending",
                withdrawal.Id, workshopId, dto.Amount);

            return new VendorWithdrawalResultDto
            {
                Success    = true,
                Message    = "Withdrawal request submitted successfully and is pending admin approval.",
                Withdrawal = MapWithdrawal(withdrawal)
            };
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static VendorWithdrawalDto MapWithdrawal(VendorWithdrawal w) =>
            new VendorWithdrawalDto
            {
                Id                  = w.Id,
                Amount              = w.Amount,
                WalletNumber        = w.WalletNumber,
                Status              = w.Status.ToString(),
                TransactionReference = w.TransactionReference,
                CreatedAt           = w.CreatedAt
            };

        private static VendorWithdrawalResultDto Fail(string message) =>
            new VendorWithdrawalResultDto { Success = false, Message = message };
    }
}

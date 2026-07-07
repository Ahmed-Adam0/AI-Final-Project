using System;
using System.Threading.Tasks;
using Graduation_Application.DTOs.WalletDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Vendor;
using Graduation_domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Graduation_Application.IRepositories;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/vendor/wallet")]
    [Authorize]
    public class VendorWalletController : ControllerBase
    {
        private readonly IVendorWalletService _walletService;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;

        public VendorWalletController(
            IVendorWalletService walletService,
            IGenaricRepositories<Workshop> workshopRepository)
        {
            _walletService     = walletService;
            _workshopRepository = workshopRepository;
        }

        /// <summary>
        /// Returns the current wallet balance for the authenticated vendor.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            try
            {
                var workshopId = await GetWorkshopIdAsync();
                var wallet = await _walletService.GetWalletAsync(workshopId);
                return Ok(wallet);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Requests a withdrawal from the vendor's wallet.
        /// </summary>
        /// <remarks>
        /// The withdrawal amount must be greater than zero and must not exceed
        /// the current AvailableBalance.
        ///
        /// The payout is sent to the specified Paymob wallet number.
        /// If the payout fails, the withdrawal is marked as Failed and no balance is deducted.
        /// </remarks>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] RequestWithdrawalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var workshopId = await GetWorkshopIdAsync();
                var result = await _walletService.RequestWithdrawalAsync(workshopId, dto);

                if (result.Success)
                    return Ok(result);

                // Business failure (e.g. insufficient balance) → 400
                return BadRequest(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Returns all withdrawal requests for the authenticated vendor, newest first.
        /// </summary>
        [HttpGet("withdrawals")]
        public async Task<IActionResult> GetWithdrawals()
        {
            try
            {
                var workshopId = await GetWorkshopIdAsync();
                var withdrawals = await _walletService.GetWithdrawalsAsync(workshopId);
                return Ok(withdrawals);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private string GetUserId()
        {
            var userId =
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return userId;
        }

        private async Task<int> GetWorkshopIdAsync()
        {
            var userId = GetUserId();

            var workshop = await _workshopRepository
                .Where(w => w.UserId == userId)
                .FirstOrDefaultAsync();

            if (workshop == null)
                throw new KeyNotFoundException("Workshop not found for the authenticated user.");

            return workshop.Id;
        }
    }
}

using System;

namespace Graduation_Application.DTOs.WalletDTO
{
    /// <summary>Vendor wallet summary returned by GET /api/vendor/wallet</summary>
    public class VendorWalletDto
    {
        public decimal AvailableBalance { get; set; }
        public decimal TotalWithdrawn   { get; set; }
    }

    /// <summary>Payload for POST /api/vendor/wallet/withdraw</summary>
    public class RequestWithdrawalDto
    {
        public decimal Amount       { get; set; }
        public string WalletNumber  { get; set; } = null!;
    }

    /// <summary>Single withdrawal record returned in the history list.</summary>
    public class VendorWithdrawalDto
    {
        public int     Id                   { get; set; }
        public decimal Amount               { get; set; }
        public string  WalletNumber         { get; set; } = null!;
        public string  Status               { get; set; } = null!;
        public string? TransactionReference  { get; set; }
        public DateTime CreatedAt           { get; set; }
    }

    /// <summary>Response returned after a withdrawal request attempt.</summary>
    public class VendorWithdrawalResultDto
    {
        public bool   Success    { get; set; }
        public string Message    { get; set; } = null!;
        public VendorWithdrawalDto? Withdrawal { get; set; }
    }

    /// <summary>Withdrawal record with Workshop context details returned to the Admin.</summary>
    public class AdminWithdrawalDto
    {
        public int      Id                   { get; set; }
        public int      WorkshopId           { get; set; }
        public string   WorkshopNameEn       { get; set; } = null!;
        public string   WorkshopNameAr       { get; set; } = null!;
        public decimal  Amount               { get; set; }
        public string   WalletNumber         { get; set; } = null!;
        public string   Status               { get; set; } = null!;
        public string?  TransactionReference { get; set; }
        public DateTime CreatedAt            { get; set; }
    }
}

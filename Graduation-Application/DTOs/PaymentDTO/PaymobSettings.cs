using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobSettings
    {
        public string ApiKey { get; set; }
        public int CardIntegrationId { get; set; }
        public int WalletIntegrationId { get; set; }
        public int IframeId { get; set; }
        public string HmacSecret { get; set; }
        /// <summary>API key for the Paymob Wallet Payout endpoint. Configure in appsettings.json under Paymob:WalletPayoutApiKey.</summary>
        public string? WalletPayoutApiKey { get; set; }
        /// <summary>Integration ID for the Paymob Wallet Payout integration. Configure in appsettings.json under Paymob:WalletPayoutIntegrationId.</summary>
        public int? WalletPayoutIntegrationId { get; set; }
    }
}

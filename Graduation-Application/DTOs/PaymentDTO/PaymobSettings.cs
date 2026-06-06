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
    }
}

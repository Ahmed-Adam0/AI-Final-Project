using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.IServices
{
    public interface IPaymobWebhookService
    {
        Task ProcessAsync(string rawPayload, string hmacHeader);
    }
}

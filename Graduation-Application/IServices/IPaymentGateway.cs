namespace Graduation_Application.IServices
{
    public interface IPaymentGateway
    {
        Task<string> CreatePaymentUrlAsync(
            int orderId,
            decimal amount,
            string firstName,
            string lastName,
            string email,
            string phone);
    }
}

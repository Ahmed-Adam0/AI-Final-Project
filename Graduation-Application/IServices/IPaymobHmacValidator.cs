namespace Graduation_Application.IServices
{
    public interface IPaymobHmacValidator
    {
        bool Validate(string hmacHeader, string payload);
    }
}

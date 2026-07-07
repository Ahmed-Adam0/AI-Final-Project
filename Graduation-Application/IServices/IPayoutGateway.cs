using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    /// <summary>
    /// Abstraction for sending vendor payouts to an external payment provider.
    /// Concrete implementation uses Paymob Wallet Payout API.
    /// Decouples <see cref="IVendorWalletService"/> from provider-specific details.
    /// </summary>
    public interface IPayoutGateway
    {
        /// <summary>
        /// Sends a payout of <paramref name="amount"/> EGP to the specified mobile <paramref name="walletNumber"/>.
        /// </summary>
        /// <returns>The provider's transaction reference string on success.</returns>
        /// <exception cref="System.Exception">Thrown when the payout fails.</exception>
        Task<string> SendPayoutAsync(decimal amount, string walletNumber);
    }
}

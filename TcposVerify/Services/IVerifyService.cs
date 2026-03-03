using TcposVerify.Models;

namespace TcposVerify.Services
{
    public interface IVerifyService
    {
        Task<VerifyViewModel> VerifyAsync(VerifyRequest req, string uniqueIdentifier, string queryString);
    }
}

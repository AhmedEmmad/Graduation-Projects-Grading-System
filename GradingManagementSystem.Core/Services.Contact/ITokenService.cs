using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Services.Contact
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user); // Signature Of Function
    }
}

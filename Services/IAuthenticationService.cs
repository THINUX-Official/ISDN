using ISDN.Models;

namespace ISDN.Services
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthenticationService
    {
        Task<(bool Success, string Token, User? User, string Message)> LoginAsync(string email, string password, string ipAddress);
        Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, string roleName = "CUSTOMER");
        Task<bool> ValidateTokenAsync(string token);
        Task RevokeTokenAsync(string token);
        Task<User?> GetUserFromTokenAsync(string token);
    }
}

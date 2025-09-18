// Application/Interfaces/IAuthService.cs
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        // Authentication methods
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> LogoutAsync(string refreshToken);

        // Token validation
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);

        // Password utilities
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);

        // Token revocation
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task<bool> RevokeAllUserTokensAsync(System.Guid userId);
    }
}
// Application/Interfaces/IJwtService.cs
using System;
using System.Security.Claims;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IJwtService
    {
        // Token generation
        string GenerateAccessToken(UserDto user);
        string GenerateRefreshToken();

        // Token validation
        ClaimsPrincipal ValidateToken(string token);
        bool IsTokenValid(string token);
        bool IsRefreshTokenValid(string refreshToken);

        // Token information extraction
        string GetUserIdFromToken(string token);
        string GetUsernameFromToken(string token);
        string GetRoleFromToken(string token);
        DateTime GetTokenExpirationDate(string token);

        // Token utilities
        bool IsTokenExpired(string token);
        TimeSpan GetTokenRemainingTime(string token);
    }
}
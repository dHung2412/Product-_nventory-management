// Application/Services/AuthService.cs
using System;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));

            // Find user by username or email
            var user = await _userRepository.GetByUsernameAsync(loginDto.UsernameOrEmail) ??
                      await _userRepository.GetByEmailAsync(loginDto.UsernameOrEmail);

            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("Invalid credentials or inactive account");

            // Verify password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials");

            // Generate tokens
            var userDto = MapToUserDto(user);
            var accessToken = _jwtService.GenerateAccessToken(userDto);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _jwtService.GetTokenExpirationDate(accessToken),
                User = userDto
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
                throw new ArgumentNullException(nameof(registerDto));

            // Check if username or email already exists
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(registerDto.Username);
            if (existingUserByUsername != null)
                throw new InvalidOperationException("Username already exists");

            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null)
                throw new InvalidOperationException("Email already exists");

            // Hash password
            var user = new User(registerDto.Username, registerDto.Email, "", Role.Employee);
            var hashedPassword = _passwordHasher.HashPassword(user, registerDto.Password);

            // Create user with hashed password
            var newUser = new User(registerDto.Username, registerDto.Email, hashedPassword, Role.Employee);
            var createdUser = await _userRepository.AddAsync(newUser);

            // Generate tokens
            var userDto = MapToUserDto(createdUser);
            var accessToken = _jwtService.GenerateAccessToken(userDto);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _jwtService.GetTokenExpirationDate(accessToken),
                User = userDto
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            if (refreshTokenDto == null)
                throw new ArgumentNullException(nameof(refreshTokenDto));

            if (!_jwtService.IsRefreshTokenValid(refreshTokenDto.RefreshToken))
                throw new UnauthorizedAccessException("Invalid refresh token");

            // In a real implementation, you would store refresh tokens in database
            // and validate them against stored tokens
            // For now, we'll assume the refresh token is valid

            // Extract user information from the refresh token or stored data
            // This is a simplified implementation
            throw new NotImplementedException("Refresh token validation requires database storage implementation");
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            // In a real implementation, you would revoke the refresh token
            // by removing it from the database or marking it as revoked
            return await Task.FromResult(true);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            return await Task.FromResult(_jwtService.IsTokenValid(token));
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            return await Task.FromResult(_jwtService.IsRefreshTokenValid(refreshToken));
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var tempUser = new User("temp", "temp@temp.com", "", Role.Employee);
            return _passwordHasher.HashPassword(tempUser, password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            var tempUser = new User("temp", "temp@temp.com", hashedPassword, Role.Employee);
            var result = _passwordHasher.VerifyHashedPassword(tempUser, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            // In a real implementation, you would revoke the specific refresh token
            // by removing it from the database or marking it as revoked
            return await Task.FromResult(true);
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            // In a real implementation, you would revoke all refresh tokens for the user
            // by removing them from the database or marking them as revoked
            return await Task.FromResult(true);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                IsActive = user.IsActive
            };
        }
    }
}
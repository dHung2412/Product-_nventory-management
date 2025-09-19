// API/Controllers/AuthController.cs
using System;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Login with username/email and password
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.LoginAsync(loginDto);
                
                _logger.LogInformation("User {Username} logged in successfully", loginDto.UsernameOrEmail);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed login attempt for {Username}: {Message}", loginDto.UsernameOrEmail, ex.Message);
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.UsernameOrEmail);
                return BadRequest(new { message = "Login failed" });
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registerDto">User registration information</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RegisterAsync(registerDto);
                
                _logger.LogInformation("New user {Username} registered successfully", registerDto.Username);
                
                return CreatedAtAction(nameof(Register), new { id = result.User.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed for {Username}: {Message}", registerDto.Username, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", registerDto.Username);
                return BadRequest(new { message = "Registration failed" });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token</param>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RefreshTokenAsync(refreshTokenDto);
                
                _logger.LogInformation("Token refreshed successfully");
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed token refresh: {Message}", ex.Message);
                return Unauthorized(new { message = "Invalid refresh token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest(new { message = "Token refresh failed" });
            }
        }

        /// <summary>
        /// Logout and revoke refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
                
                if (result)
                {
                    _logger.LogInformation("User logged out successfully");
                    return Ok(new { message = "Logged out successfully" });
                }
                
                return BadRequest(new { message = "Logout failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(new { message = "Logout failed" });
            }
        }

        /// <summary>
        /// Validate current access token
        /// </summary>
        /// <returns>Token validation status</returns>
        [HttpGet("validate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var isValid = await _authService.ValidateTokenAsync(token);
                
                if (isValid)
                {
                    return Ok(new { message = "Token is valid", isValid = true });
                }
                
                return Unauthorized(new { message = "Token is invalid", isValid = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token validation");
                return Unauthorized(new { message = "Token validation failed", isValid = false });
            }
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>Current user details</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var username = User.FindFirst("username")?.Value;
                var role = User.FindFirst("role")?.Value;
                var email = User.FindFirst("email")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var userDto = new UserDto
                {
                    Id = Guid.Parse(userId),
                    Username = username,
                    Email = email,
                    Role = Enum.Parse<Domain.Entities.Role>(role),
                    RoleName = role,
                    IsActive = true // Assuming if token is valid, user is active
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user information");
                return Unauthorized();
            }
        }
    }
}
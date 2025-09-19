// API/Controllers/UserController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user");
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User details</returns>
        [HttpGet("username/{username}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetByUsernameAsync(username);
                if (user == null)
                    return NotFound($"User with username '{username}' not found");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with username {Username}", username);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user");
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User details</returns>
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                    return NotFound($"User with email '{email}' not found");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with email {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user");
            }
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns>List of active users</returns>
        [HttpGet("active")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active users");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving active users");
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns>List of users with specified role</returns>
        [HttpGet("role/{role}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(Role role)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users with role {Role}", role);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users by role");
            }
        }

        /// <summary>
        /// Search users by term
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching users</returns>
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest("Search term cannot be empty");

                var users = await _userService.SearchUsersAsync(searchTerm);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching users with term {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching users");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user details</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userService.CreateAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User creation failed due to business rule violation");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user");
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">User update data</param>
        /// <returns>Updated user details</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userService.UpdateAsync(id, updateUserDto);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "User update failed - user not found");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User update failed due to business rule violation");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user");
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteAsync(id);
                if (!result)
                    return NotFound($"User with ID {id} not found");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User deletion failed due to business rule violation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user");
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="changePasswordDto">Password change data</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
                if (!result)
                    return NotFound($"User with ID {id} not found");

                return Ok(new { message = "Password changed successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Password change failed - user not found");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password change failed due to business rule violation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while changing the password");
            }
        }

        /// <summary>
        /// Reset user password (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="newPassword">New password</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ResetPassword(Guid id, [FromBody] string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                    return BadRequest("New password cannot be empty");

                var result = await _userService.ResetPasswordAsync(id, newPassword);
                if (!result)
                    return NotFound($"User with ID {id} not found");

                return Ok(new { message = "Password reset successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Password reset failed - user not found");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while resetting the password");
            }
        }

        /// <summary>
        /// Activate a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ActivateUser(Guid id)
        {
            try
            {
                var result = await _userService.ActivateUserAsync(id);
                if (!result)
                    return NotFound($"User with ID {id} not found");

                return Ok(new { message = "User activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while activating the user");
            }
        }

        /// <summary>
        /// Deactivate a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeactivateUser(Guid id)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(id);
                if (!result)
                    return NotFound($"User with ID {id} not found");

                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deactivating the user");
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>Existence status</returns>
        [HttpGet("check-username/{username}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckUsernameExists(string username)
        {
            try
            {
                var exists = await _userService.UsernameExistsAsync(username);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking username existence for {Username}", username);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking username");
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Existence status</returns>
        [HttpGet("check-email/{email}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking email existence for {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking email");
            }
        }

        /// <summary>
        /// Get total users count
        /// </summary>
        /// <returns>Total users count</returns>
        [HttpGet("stats/total")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> GetTotalUsersCount()
        {
            try
            {
                var count = await _userService.GetTotalUsersCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving total users count");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user statistics");
            }
        }

        /// <summary>
        /// Get active users count
        /// </summary>
        /// <returns>Active users count</returns>
        [HttpGet("stats/active")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> GetActiveUsersCount()
        {
            try
            {
                var count = await _userService.GetActiveUsersCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active users count");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user statistics");
            }
        }

        /// <summary>
        /// Check if user can be deleted
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Deletion possibility status</returns>
        [HttpGet("{id}/can-delete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> CanDeleteUser(Guid id)
        {
            try
            {
                var exists = await _userService.ExistsAsync(id);
                if (!exists)
                    return NotFound($"User with ID {id} not found");

                var canDelete = await _userService.CanDeleteUserAsync(id);
                return Ok(canDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if user with ID {UserId} can be deleted", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking deletion possibility");
            }
        }
    }
}
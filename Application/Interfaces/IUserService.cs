// Application/Interfaces/IUserService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        // Basic CRUD operations
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<UserDto?> GetByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<IEnumerable<UserDto>> GetActiveUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(Role role);
        Task<UserDto> CreateAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto);
        Task<bool> DeleteAsync(Guid id);

        // Password management
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
        Task<bool> ResetPasswordAsync(Guid userId, string newPassword);

        // Validation methods
        Task<bool> ExistsAsync(Guid id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> ValidatePasswordAsync(Guid userId, string password);

        // User status management
        Task<bool> ActivateUserAsync(Guid userId);
        Task<bool> DeactivateUserAsync(Guid userId);

        // Statistics
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();

        // Business logic methods
        Task<bool> CanDeleteUserAsync(Guid userId);
        Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm);
    }
}
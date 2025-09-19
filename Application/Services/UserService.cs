// Application/Services/UserService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        #region Basic CRUD Operations

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var user = await _userRepository.GetByUsernameAsync(username);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
        {
            var users = await _userRepository.GetActiveUsersAsync();
            return users.Select(MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(Role role)
        {
            var users = await _userRepository.GetUsersByRoleAsync(role);
            return users.Select(MapToDto);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
        {
            if (createUserDto == null)
                throw new ArgumentNullException(nameof(createUserDto));

            // Check if username already exists
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(createUserDto.Username);
            if (existingUserByUsername != null)
                throw new InvalidOperationException($"Username '{createUserDto.Username}' already exists");

            // Check if email already exists
            var existingUserByEmail = await _userRepository.GetByEmailAsync(createUserDto.Email);
            if (existingUserByEmail != null)
                throw new InvalidOperationException($"Email '{createUserDto.Email}' already exists");

            // Create user with hashed password
            var user = new User(createUserDto.Username, createUserDto.Email, "", createUserDto.Role, createUserDto.IsActive);
            var hashedPassword = _passwordHasher.HashPassword(user, createUserDto.Password);
            
            var newUser = new User(createUserDto.Username, createUserDto.Email, hashedPassword, createUserDto.Role, createUserDto.IsActive);
            var createdUser = await _userRepository.AddAsync(newUser);

            return MapToDto(createdUser);
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto)
        {
            if (updateUserDto == null)
                throw new ArgumentNullException(nameof(updateUserDto));

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException($"User with ID {id} not found", nameof(id));

            // Check if username already exists (excluding current user)
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(updateUserDto.Username);
            if (existingUserByUsername != null && existingUserByUsername.Id != id)
                throw new InvalidOperationException($"Username '{updateUserDto.Username}' already exists");

            // Check if email already exists (excluding current user)
            var existingUserByEmail = await _userRepository.GetByEmailAsync(updateUserDto.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != id)
                throw new InvalidOperationException($"Email '{updateUserDto.Email}' already exists");

            user.Update(updateUserDto.Username, updateUserDto.Email, user.PasswordHash, updateUserDto.Role, updateUserDto.IsActive);
            await _userRepository.UpdateAsync(user);

            return MapToDto(user);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var canDelete = await CanDeleteUserAsync(id);
            if (!canDelete)
                throw new InvalidOperationException("Cannot delete user with existing stock transactions");

            var exists = await _userRepository.ExistsAsync(id);
            if (!exists)
                return false;

            await _userRepository.DeleteAsync(id);
            return true;
        }

        #endregion

        #region Password Management

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            if (changePasswordDto == null)
                throw new ArgumentNullException(nameof(changePasswordDto));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found", nameof(userId));

            // Verify current password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, changePasswordDto.CurrentPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("Current password is incorrect");

            // Hash new password
            var newHashedPassword = _passwordHasher.HashPassword(user, changePasswordDto.NewPassword);
            user.Update(user.Username, user.Email, newHashedPassword, user.Role, user.IsActive);

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be null or empty", nameof(newPassword));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found", nameof(userId));

            // Hash new password
            var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
            user.Update(user.Username, user.Email, hashedPassword, user.Role, user.IsActive);

            await _userRepository.UpdateAsync(user);
            return true;
        }

        #endregion

        #region Validation Methods

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _userRepository.ExistsAsync(id);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _userRepository.UsernameExistsAsync(username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _userRepository.EmailExistsAsync(email);
        }

        public async Task<bool> ValidatePasswordAsync(Guid userId, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return passwordVerificationResult == PasswordVerificationResult.Success;
        }

        #endregion

        #region User Status Management

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (user.IsActive)
                return true; // Already active

            user.Update(user.Username, user.Email, user.PasswordHash, user.Role, true);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (!user.IsActive)
                return true; // Already inactive

            user.Update(user.Username, user.Email, user.PasswordHash, user.Role, false);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        #endregion

        #region Statistics

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _userRepository.GetTotalUsersCountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _userRepository.GetActiveUsersCountAsync();
        }

        #endregion

        #region Business Logic Methods

        public async Task<bool> CanDeleteUserAsync(Guid userId)
        {
            // In a real implementation, you would check if the user has any stock transactions
            // For now, we'll assume users can always be deleted
            // This should be implemented when you have the stock transaction relationships
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null;
        }

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var users = await _userRepository.GetAllAsync();
            var filteredUsers = users.Where(u =>
                u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            );

            return filteredUsers.Select(MapToDto);
        }

        #endregion

        #region Private Helper Methods

        private static UserDto MapToDto(User user)
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

        #endregion
    }
}
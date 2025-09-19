// Infrastructure/Security/PasswordHasher.cs
using System;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher<User>
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 10000;

        public string HashPassword(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Generate a random salt
            var salt = GenerateSalt();
            
            // Hash the password with the salt
            var hash = HashPasswordWithSalt(password, salt);
            
            // Combine salt and hash
            var combinedBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, combinedBytes, 0, SaltSize);
            Array.Copy(hash, 0, combinedBytes, SaltSize, HashSize);
            
            // Convert to base64 string
            return Convert.ToBase64String(combinedBytes);
        }

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrEmpty(hashedPassword))
                return PasswordVerificationResult.Failed;
            
            if (string.IsNullOrEmpty(providedPassword))
                return PasswordVerificationResult.Failed;

            try
            {
                // Convert base64 string back to bytes
                var combinedBytes = Convert.FromBase64String(hashedPassword);
                
                if (combinedBytes.Length != SaltSize + HashSize)
                    return PasswordVerificationResult.Failed;
                
                // Extract salt and hash
                var salt = new byte[SaltSize];
                var hash = new byte[HashSize];
                Array.Copy(combinedBytes, 0, salt, 0, SaltSize);
                Array.Copy(combinedBytes, SaltSize, hash, 0, HashSize);
                
                // Hash the provided password with the extracted salt
                var providedHash = HashPasswordWithSalt(providedPassword, salt);
                
                // Compare hashes
                if (AreHashesEqual(hash, providedHash))
                {
                    // Check if rehashing is needed (e.g., if iterations have changed)
                    return ShouldRehash(hashedPassword) ? 
                        PasswordVerificationResult.SuccessRehashNeeded : 
                        PasswordVerificationResult.Success;
                }
                
                return PasswordVerificationResult.Failed;
            }
            catch
            {
                return PasswordVerificationResult.Failed;
            }
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[SaltSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        private static byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256);
            
            return pbkdf2.GetBytes(HashSize);
        }

        private static bool AreHashesEqual(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;
            
            var result = 0;
            for (var i = 0; i < hash1.Length; i++)
            {
                result |= hash1[i] ^ hash2[i];
            }
            
            return result == 0;
        }

        private static bool ShouldRehash(string hashedPassword)
        {
            // In a production system, you might want to implement logic
            // to determine if the password should be rehashed
            // (e.g., if the hashing algorithm or iterations have changed)
            return false;
        }
    }

    /// <summary>
    /// Alternative simpler implementation using BCrypt
    /// Uncomment this and comment the above implementation if you prefer BCrypt
    /// Note: You'll need to install BCrypt.Net-Next NuGet package
    /// </summary>
    /*
    public class BCryptPasswordHasher : IPasswordHasher<User>
    {
        private const int WorkFactor = 12;

        public string HashPassword(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrEmpty(hashedPassword))
                return PasswordVerificationResult.Failed;
            
            if (string.IsNullOrEmpty(providedPassword))
                return PasswordVerificationResult.Failed;

            try
            {
                var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
                return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
            }
            catch
            {
                return PasswordVerificationResult.Failed;
            }
        }
    }
    */
}
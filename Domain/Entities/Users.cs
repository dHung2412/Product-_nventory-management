// Domain/Entities/User.cs
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public enum Role
    {
        Admin,
        Manager,
        Employee
    }

    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public Role Role { get; private set; }
        public bool IsActive { get; private set; }

        public ICollection<StockTransaction> StockTransactions { get; private set; } = new List<StockTransaction>();

        // Private constructor for EF Core
        private User() { }

        // Public constructor for creating new User
        public User(string username, string email, string passwordHash, Role role, bool isActive = true)
        {
            Id = Guid.NewGuid();
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            Role = role;
            IsActive = isActive;
        }

        // Update method
        public void Update(string username, string email, string passwordHash, Role role, bool isActive)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            Role = role;
            IsActive = isActive;
        }
    }
}
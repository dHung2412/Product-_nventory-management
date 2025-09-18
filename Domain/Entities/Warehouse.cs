// Domain/Entities/Warehouse.cs
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Warehouse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        // Private constructor for EF Core
        private Warehouse() { }

        // Public constructor for creating new Warehouse
        public Warehouse(string name, string address)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        // Update method
        public void Update(string name, string address)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }
    }
} 
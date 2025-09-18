// Domain/Entities/Product.cs
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Unit { get; private set; }
        public decimal Price { get; private set; }
        public string Category { get; private set; }

        public ICollection<StockItem> StockItems { get; private set; } = new List<StockItem>();
        public ICollection<StockTransaction> StockTransactions { get; private set; } = new List<StockTransaction>();

        // Private constructor for EF Core
        private Product() { }

        // Public constructor for creating new Product
        public Product(string name, string description, string unit, decimal price, string category)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            Price = price;
            Category = category ?? throw new ArgumentNullException(nameof(category));
        }

        // Update method
        public void Update(string name, string description, string unit, decimal price, string category)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            Price = price;
            Category = category ?? throw new ArgumentNullException(nameof(category));
        }
    }
}
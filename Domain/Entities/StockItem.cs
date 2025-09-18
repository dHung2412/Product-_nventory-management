// Domain/Entities/StockItem.cs
using System;

namespace Domain.Entities
{
    public class StockItem
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid WarehouseId { get; private set; }
        public int Quantity { get; private set; }

        public Product Product { get; private set; }
        public Warehouse Warehouse { get; private set; }

        // Private constructor for EF Core
        private StockItem() { }

        // Public constructor for creating new StockItem
        public StockItem(Guid productId, Guid warehouseId, int quantity)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            WarehouseId = warehouseId;
            Quantity = quantity >= 0 ? quantity : throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
        }

        // Add quantity
        public void AddQuantity(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            Quantity += amount;
        }

        // Remove quantity
        public void RemoveQuantity(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            if (Quantity - amount < 0) throw new InvalidOperationException("Cannot remove more than available quantity.");
            Quantity -= amount;
        }

        // Update quantity directly
        public void UpdateQuantity(int newQuantity)
        {
            Quantity = newQuantity >= 0 ? newQuantity : throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity cannot be negative.");
        }

        // Check if low stock (e.g., below 10)
        public bool IsLow()
        {
            return Quantity < 10;
        }

        // Check if over stock (e.g., above 100)
        public bool IsOver()
        {
            return Quantity > 100;
        }
    }
}
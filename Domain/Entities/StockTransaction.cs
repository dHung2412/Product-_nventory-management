// Domain/Entities/StockTransaction.cs
using System;

namespace Domain.Entities
{
    public enum TransactionType
    {
        Import,
        Export,
        Adjustment
    }

    public class StockTransaction
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid WarehouseId { get; private set; }
        public TransactionType Type { get; private set; }
        public int Quantity { get; private set; }
        public string Reason { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public Guid UserId { get; private set; }

        public Product Product { get; private set; }
        public Warehouse Warehouse { get; private set; }
        public User User { get; private set; }

        // Private constructor for EF Core
        private StockTransaction() { }

        // Public constructor for creating new StockTransaction
        public StockTransaction(Guid productId, Guid warehouseId, TransactionType type, int quantity, string reason, Guid userId)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            WarehouseId = warehouseId;
            Type = type;
            Quantity = quantity;
            Reason = reason;
            TransactionDate = DateTime.UtcNow;
            UserId = userId;
        }

        // Update method (limited, as transactions are immutable in many systems, but allowing update for reason or quantity if needed)
        public void Update(string reason, int quantity)
        {
            Reason = reason;
            Quantity = quantity;
        }
    }
}
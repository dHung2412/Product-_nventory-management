// Application/DTOs/StockTransactionDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.DTOs
{
    public class StockTransactionDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public DateTime TransactionDate { get; set; }
        public Guid UserId { get; set; }

        // Navigation properties for display
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public string Username { get; set; }
        public string TypeName { get; set; }
    }

    public class CreateStockTransactionDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class UpdateStockTransactionDto
    {
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public int Quantity { get; set; }
    }

    // Specific DTOs for different transaction types
    public class ImportStockDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class ExportStockDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class AdjustStockDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}
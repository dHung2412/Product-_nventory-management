// Application/DTOs/StockItemDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class StockItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public int Quantity { get; set; }
        public bool IsLow { get; set; }
        public bool IsOver { get; set; }

        // Navigation properties for display
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public string ProductUnit { get; set; }
    }

    public class CreateStockItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
        public int Quantity { get; set; }
    }

    public class UpdateStockItemQuantityDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
        public int Quantity { get; set; }
    }

    public class AddStockDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be positive")]
        public int Amount { get; set; }
    }

    public class RemoveStockDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be positive")]
        public int Amount { get; set; }
    }
}
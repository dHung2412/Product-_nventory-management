// Application/Interfaces/IStockService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IStockService
    {
        // Stock Item operations
        Task<StockItemDto?> GetStockItemByIdAsync(Guid id);
        Task<IEnumerable<StockItemDto>> GetAllStockItemsAsync();
        Task<IEnumerable<StockItemDto>> GetStockItemsByProductIdAsync(Guid productId);
        Task<IEnumerable<StockItemDto>> GetStockItemsByWarehouseIdAsync(Guid warehouseId);
        Task<StockItemDto> CreateStockItemAsync(CreateStockItemDto createStockItemDto);
        Task<bool> DeleteStockItemAsync(Guid id);

        // Stock quantity operations
        Task<StockItemDto> UpdateStockQuantityAsync(Guid stockItemId, UpdateStockItemQuantityDto updateDto);
        Task<StockItemDto> AddStockAsync(Guid stockItemId, AddStockDto addStockDto);
        Task<StockItemDto> RemoveStockAsync(Guid stockItemId, RemoveStockDto removeStockDto);

        // Stock Transaction operations
        Task<StockTransactionDto?> GetTransactionByIdAsync(Guid id);
        Task<IEnumerable<StockTransactionDto>> GetAllTransactionsAsync();
        Task<IEnumerable<StockTransactionDto>> GetTransactionsByProductIdAsync(Guid productId);
        Task<IEnumerable<StockTransactionDto>> GetTransactionsByWarehouseIdAsync(Guid warehouseId);
        Task<IEnumerable<StockTransactionDto>> GetTransactionsByUserIdAsync(Guid userId);
        Task<IEnumerable<StockTransactionDto>> GetTransactionsByTypeAsync(TransactionType type);

        // Business operations
        Task<StockTransactionDto> ImportStockAsync(ImportStockDto importStockDto);
        Task<StockTransactionDto> ExportStockAsync(ExportStockDto exportStockDto);
        Task<StockTransactionDto> AdjustStockAsync(AdjustStockDto adjustStockDto);

        // Validation and checks
        Task<bool> HasSufficientStockAsync(Guid productId, Guid warehouseId, int requiredQuantity);
        Task<int> GetAvailableStockAsync(Guid productId, Guid warehouseId);

        // Reports and Analytics
        Task<IEnumerable<StockItemDto>> GetLowStockItemsAsync();
        Task<IEnumerable<StockItemDto>> GetOverStockItemsAsync();
        Task<IEnumerable<StockTransactionDto>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<string, int>> GetStockSummaryByWarehouseAsync(Guid warehouseId);
        
        // Statistics
        Task<int> GetTotalStockItemsCountAsync();
        Task<int> GetTotalTransactionsCountAsync();
    }
}
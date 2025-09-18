// Domain/Interfaces/IStockItemRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IStockItemRepository
    {
        // Basic query operations
        Task<StockItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<StockItem>> GetAllAsync();
        Task<IEnumerable<StockItem>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<StockItem>> GetByWarehouseIdAsync(Guid warehouseId);

        Task<StockItem> AddAsync(StockItem stockItem);
        Task UpdateAsync(StockItem stockItem);
        Task DeleteAsync(Guid id);

        // Existence checks
        Task<bool> ExistsAsync(Guid id);
        
        // Statistics
        Task<int> GetTotalStockItemsCountAsync();
    }
}
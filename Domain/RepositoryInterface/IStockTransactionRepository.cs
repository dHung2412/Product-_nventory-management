// Domain/Interfaces/IStockTransactionRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IStockTransactionRepository
    {
        // Basic query operations
        Task<StockTransaction?> GetByIdAsync(Guid id);
        Task<IEnumerable<StockTransaction>> GetAllAsync();
        Task<IEnumerable<StockTransaction>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<StockTransaction>> GetByWarehouseIdAsync(Guid warehouseId);
        Task<IEnumerable<StockTransaction>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<StockTransaction>> GetByTypeAsync(TransactionType type);

        Task<StockTransaction> AddAsync(StockTransaction stockTransaction);
        Task UpdateAsync(StockTransaction stockTransaction);
        Task DeleteAsync(Guid id);

        // Existence checks
        Task<bool> ExistsAsync(Guid id);
        
        // Statistics
        Task<int> GetTotalTransactionsCountAsync();
    }
}
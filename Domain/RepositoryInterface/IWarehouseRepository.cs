// Domain/Interfaces/IWarehouseRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IWarehouseRepository
    {
        // Basic query operations
        Task<Warehouse?> GetByIdAsync(Guid id);
        Task<IEnumerable<Warehouse>> GetAllAsync();

        Task<Warehouse> AddAsync(Warehouse warehouse);
        Task UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(Guid id);

        // Existence checks
        Task<bool> ExistsAsync(Guid id);
        
        // Statistics
        Task<int> GetTotalWarehousesCountAsync();
    }
}
// Application/Interfaces/IWarehouseService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IWarehouseService
    {
        // Basic CRUD operations
        Task<WarehouseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<WarehouseDto>> GetAllAsync();
        Task<WarehouseDto> CreateAsync(CreateWarehouseDto createWarehouseDto);
        Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto updateWarehouseDto);
        Task<bool> DeleteAsync(Guid id);

        // Validation methods
        Task<bool> ExistsAsync(Guid id);

        // Statistics
        Task<int> GetTotalWarehousesCountAsync();

        // Business logic methods
        Task<IEnumerable<StockItemDto>> GetWarehouseStockAsync(Guid warehouseId);
        Task<bool> CanDeleteWarehouseAsync(Guid warehouseId);
    }
}
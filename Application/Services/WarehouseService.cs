// Application/Services/WarehouseService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStockItemRepository _stockItemRepository;

        public WarehouseService(
            IWarehouseRepository warehouseRepository,
            IStockItemRepository stockItemRepository)
        {
            _warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
            _stockItemRepository = stockItemRepository ?? throw new ArgumentNullException(nameof(stockItemRepository));
        }

        public async Task<WarehouseDto?> GetByIdAsync(Guid id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            return warehouse == null ? null : await MapWarehouseToDtoAsync(warehouse);
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
                var dtos = new List<WarehouseDto>();
                foreach (var w in warehouses)
                {
                    dtos.Add(await MapWarehouseToDtoAsync(w));
                }
                return dtos; 
        }

        public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto createWarehouseDto)
        {
            if (createWarehouseDto == null)
                throw new ArgumentNullException(nameof(createWarehouseDto));

            var warehouse = new Warehouse(
                createWarehouseDto.Name,
                createWarehouseDto.Address
            );

            var createdWarehouse = await _warehouseRepository.AddAsync(warehouse);
            return MapToDto(createdWarehouse);
        }

        public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto updateWarehouseDto)
        {
            if (updateWarehouseDto == null)
                throw new ArgumentNullException(nameof(updateWarehouseDto));

            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new ArgumentException($"Warehouse with ID {id} not found", nameof(id));

            warehouse.Update(updateWarehouseDto.Name, updateWarehouseDto.Address);

            await _warehouseRepository.UpdateAsync(warehouse);
            return MapToDto(warehouse);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var canDelete = await CanDeleteWarehouseAsync(id);
            if (!canDelete)
                throw new InvalidOperationException("Cannot delete warehouse that has stock items");

            var exists = await _warehouseRepository.ExistsAsync(id);
            if (!exists)
                return false;

            await _warehouseRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _warehouseRepository.ExistsAsync(id);
        }

        public async Task<int> GetTotalWarehousesCountAsync()
        {
            return await _warehouseRepository.GetTotalWarehousesCountAsync();
        }

        public async Task<IEnumerable<StockItemDto>> GetWarehouseStockAsync(Guid warehouseId)
        {
            var stockItems = await _stockItemRepository.GetByWarehouseIdAsync(warehouseId);
            return stockItems.Select(MapStockItemToDto);
        }

        public async Task<bool> CanDeleteWarehouseAsync(Guid warehouseId)
        {
            var stockItems = await _stockItemRepository.GetByWarehouseIdAsync(warehouseId);
            return !stockItems.Any();
        }

        private static WarehouseDto MapToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address
            };
        }

        private static StockItemDto MapStockItemToDto(StockItem stockItem)
        {
            return new StockItemDto
            {
                Id = stockItem.Id,
                ProductId = stockItem.ProductId,
                WarehouseId = stockItem.WarehouseId,
                Quantity = stockItem.Quantity,
                IsLow = stockItem.IsLow(),
                IsOver = stockItem.IsOver(),
                ProductName = stockItem.Product?.Name,
                WarehouseName = stockItem.Warehouse?.Name,
                ProductUnit = stockItem.Product?.Unit
            };
        }

        private async Task<WarehouseDto> MapWarehouseToDtoAsync(Warehouse warehouse)
        {
            var stockItems = await _stockItemRepository.GetByWarehouseIdAsync(warehouse.Id);
            var totalStock = stockItems.Sum(si => si.Quantity);

            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                TotalStock = totalStock
            };
        }
    }
}
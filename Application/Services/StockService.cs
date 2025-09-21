// Application/Services/StockService.cs
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
    public class StockService : IStockService
    {
        private readonly IStockItemRepository _stockItemRepository;
        private readonly IStockTransactionRepository _stockTransactionRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IUserRepository _userRepository;

        public StockService(
            IStockItemRepository stockItemRepository,
            IStockTransactionRepository stockTransactionRepository,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository,
            IUserRepository userRepository)
        {
            _stockItemRepository = stockItemRepository ?? throw new ArgumentNullException(nameof(stockItemRepository));
            _stockTransactionRepository = stockTransactionRepository ?? throw new ArgumentNullException(nameof(stockTransactionRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        #region Stock Item Operations

        public async Task<StockItemDto?> GetStockItemByIdAsync(Guid id)
        {
            var stockItem = await _stockItemRepository.GetByIdAsync(id);
            return stockItem == null ? null : await MapStockItemToDtoAsync(stockItem);
        }

        public async Task<IEnumerable<StockItemDto>> GetAllStockItemsAsync()
        {
            var stockItems = await _stockItemRepository.GetAllAsync();
            var dtos = new List<StockItemDto>();
            foreach (var si in stockItems)
            {
                dtos.Add(await MapStockItemToDtoAsync(si));
            }
            return dtos;
        }

        public async Task<IEnumerable<StockItemDto>> GetStockItemsByProductIdAsync(Guid productId)
        {
            var stockItems = await _stockItemRepository.GetByProductIdAsync(productId);
            var tasks = stockItems.Select(MapStockItemToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<StockItemDto>> GetStockItemsByWarehouseIdAsync(Guid warehouseId)
        {
            var stockItems = await _stockItemRepository.GetByWarehouseIdAsync(warehouseId);
            var tasks = stockItems.Select(MapStockItemToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<StockItemDto> CreateStockItemAsync(CreateStockItemDto createStockItemDto)
        {
            if (createStockItemDto == null)
                throw new ArgumentNullException(nameof(createStockItemDto));

            // Validate product and warehouse exist
            await ValidateProductExistsAsync(createStockItemDto.ProductId);
            await ValidateWarehouseExistsAsync(createStockItemDto.WarehouseId);

            var stockItem = new StockItem(
                createStockItemDto.ProductId,
                createStockItemDto.WarehouseId,
                createStockItemDto.Quantity
            );

            var createdStockItem = await _stockItemRepository.AddAsync(stockItem);
            return await MapStockItemToDtoAsync(createdStockItem);
        }

        public async Task<bool> DeleteStockItemAsync(Guid id)
        {
            var exists = await _stockItemRepository.ExistsAsync(id);
            if (!exists)
                return false;

            await _stockItemRepository.DeleteAsync(id);
            return true;
        }

        #endregion

        #region Stock Quantity Operations

        public async Task<StockItemDto> UpdateStockQuantityAsync(Guid stockItemId, UpdateStockItemQuantityDto updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var stockItem = await GetStockItemEntityAsync(stockItemId);
            stockItem.UpdateQuantity(updateDto.Quantity);

            await _stockItemRepository.UpdateAsync(stockItem);
            return await MapStockItemToDtoAsync(stockItem);
        }

        public async Task<StockItemDto> AddStockAsync(Guid stockItemId, AddStockDto addStockDto)
        {
            if (addStockDto == null)
                throw new ArgumentNullException(nameof(addStockDto));

            var stockItem = await GetStockItemEntityAsync(stockItemId);
            stockItem.AddQuantity(addStockDto.Amount);

            await _stockItemRepository.UpdateAsync(stockItem);
            return await MapStockItemToDtoAsync(stockItem);
        }

        public async Task<StockItemDto> RemoveStockAsync(Guid stockItemId, RemoveStockDto removeStockDto)
        {
            if (removeStockDto == null)
                throw new ArgumentNullException(nameof(removeStockDto));

            var stockItem = await GetStockItemEntityAsync(stockItemId);
            stockItem.RemoveQuantity(removeStockDto.Amount);

            await _stockItemRepository.UpdateAsync(stockItem);
            return await MapStockItemToDtoAsync(stockItem);
        }

        #endregion

        #region Stock Transaction Operations

        public async Task<StockTransactionDto?> GetTransactionByIdAsync(Guid id)
        {
            var transaction = await _stockTransactionRepository.GetByIdAsync(id);
            return transaction == null ? null : await MapTransactionToDtoAsync(transaction);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetAllTransactionsAsync()
        {
            var transactions = await _stockTransactionRepository.GetAllAsync();
            var tasks = transactions.Select(MapTransactionToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetTransactionsByProductIdAsync(Guid productId)
        {
            var transactions = await _stockTransactionRepository.GetByProductIdAsync(productId);
            var tasks = transactions.Select(MapTransactionToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetTransactionsByWarehouseIdAsync(Guid warehouseId)
        {
            var transactions = await _stockTransactionRepository.GetByWarehouseIdAsync(warehouseId);
            var tasks = transactions.Select(MapTransactionToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetTransactionsByUserIdAsync(Guid userId)
        {
            var transactions = await _stockTransactionRepository.GetByUserIdAsync(userId);
            var tasks = transactions.Select(MapTransactionToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetTransactionsByTypeAsync(TransactionType type)
        {
            var transactions = await _stockTransactionRepository.GetByTypeAsync(type);
            var tasks = transactions.Select(MapTransactionToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        #endregion

        #region Business Operations

        public async Task<StockTransactionDto> ImportStockAsync(ImportStockDto importStockDto)
        {
            if (importStockDto == null)
                throw new ArgumentNullException(nameof(importStockDto));

            await ValidateProductExistsAsync(importStockDto.ProductId);
            await ValidateWarehouseExistsAsync(importStockDto.WarehouseId);
            await ValidateUserExistsAsync(importStockDto.UserId);

            // Create transaction
            var transaction = new StockTransaction(
                importStockDto.ProductId,
                importStockDto.WarehouseId,
                TransactionType.Import,
                importStockDto.Quantity,
                importStockDto.Reason ?? "Stock import",
                importStockDto.UserId
            );

            var createdTransaction = await _stockTransactionRepository.AddAsync(transaction);

            // Update stock item
            await UpdateStockForTransaction(importStockDto.ProductId, importStockDto.WarehouseId, importStockDto.Quantity);

            return await MapTransactionToDtoAsync(createdTransaction);
        }

        public async Task<StockTransactionDto> ExportStockAsync(ExportStockDto exportStockDto)
        {
            if (exportStockDto == null)
                throw new ArgumentNullException(nameof(exportStockDto));

            await ValidateProductExistsAsync(exportStockDto.ProductId);
            await ValidateWarehouseExistsAsync(exportStockDto.WarehouseId);
            await ValidateUserExistsAsync(exportStockDto.UserId);

            // Check sufficient stock
            var hasSufficientStock = await HasSufficientStockAsync(
                exportStockDto.ProductId,
                exportStockDto.WarehouseId,
                exportStockDto.Quantity);

            if (!hasSufficientStock)
                throw new InvalidOperationException("Insufficient stock for export operation");

            // Create transaction
            var transaction = new StockTransaction(
                exportStockDto.ProductId,
                exportStockDto.WarehouseId,
                TransactionType.Export,
                exportStockDto.Quantity,
                exportStockDto.Reason ?? "Stock export",
                exportStockDto.UserId
            );

            var createdTransaction = await _stockTransactionRepository.AddAsync(transaction);

            // Update stock item
            await UpdateStockForTransaction(exportStockDto.ProductId, exportStockDto.WarehouseId, -exportStockDto.Quantity);

            return await MapTransactionToDtoAsync(createdTransaction);
        }

        public async Task<StockTransactionDto> AdjustStockAsync(AdjustStockDto adjustStockDto)
        {
            if (adjustStockDto == null)
                throw new ArgumentNullException(nameof(adjustStockDto));

            await ValidateProductExistsAsync(adjustStockDto.ProductId);
            await ValidateWarehouseExistsAsync(adjustStockDto.WarehouseId);
            await ValidateUserExistsAsync(adjustStockDto.UserId);

            var currentStock = await GetAvailableStockAsync(adjustStockDto.ProductId, adjustStockDto.WarehouseId);
            var adjustment = adjustStockDto.Quantity - currentStock;

            // Create transaction
            var transaction = new StockTransaction(
                adjustStockDto.ProductId,
                adjustStockDto.WarehouseId,
                TransactionType.Adjustment,
                adjustment,
                adjustStockDto.Reason ?? "Stock adjustment",
                adjustStockDto.UserId
            );

            var createdTransaction = await _stockTransactionRepository.AddAsync(transaction);

            // Update stock item to exact quantity
            await SetStockQuantity(adjustStockDto.ProductId, adjustStockDto.WarehouseId, adjustStockDto.Quantity);

            return await MapTransactionToDtoAsync(createdTransaction);
        }

        #endregion

        #region Validation and Checks

        public async Task<bool> HasSufficientStockAsync(Guid productId, Guid warehouseId, int requiredQuantity)
        {
            var availableStock = await GetAvailableStockAsync(productId, warehouseId);
            return availableStock >= requiredQuantity;
        }

        public async Task<int> GetAvailableStockAsync(Guid productId, Guid warehouseId)
        {
            var stockItems = await _stockItemRepository.GetByProductIdAsync(productId);
            var stockItem = stockItems.FirstOrDefault(si => si.WarehouseId == warehouseId);
            return stockItem?.Quantity ?? 0;
        }

        #endregion

        #region Reports and Analytics

        public async Task<IEnumerable<StockItemDto>> GetLowStockItemsAsync()
        {
            var stockItems = await _stockItemRepository.GetAllAsync();
            var lowStockItems = stockItems.Where(si => si.IsLow());
            var tasks = lowStockItems.Select(MapStockItemToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<StockItemDto>> GetOverStockItemsAsync()
        {
            var stockItems = await _stockItemRepository.GetAllAsync();
            var overStockItems = stockItems.Where(si => si.IsOver());
            var tasks = overStockItems.Select(MapStockItemToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var transactions = await _stockTransactionRepository.GetAllAsync();
            var filteredTransactions = transactions.Where(t => 
                t.TransactionDate >= fromDate && t.TransactionDate <= toDate);
            var tasks = filteredTransactions.Select(MapTransactionToDtoAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<Dictionary<string, int>> GetStockSummaryByWarehouseAsync(Guid warehouseId)
        {
            var stockItems = await _stockItemRepository.GetByWarehouseIdAsync(warehouseId);
            var summary = new Dictionary<string, int>
            {
                { "TotalProducts", stockItems.Count() },
                { "TotalQuantity", stockItems.Sum(si => si.Quantity) },
                { "LowStockItems", stockItems.Count(si => si.IsLow()) },
                { "OverStockItems", stockItems.Count(si => si.IsOver()) }
            };
            return summary;
        }

        #endregion

        #region Statistics

        public async Task<int> GetTotalStockItemsCountAsync()
        {
            return await _stockItemRepository.GetTotalStockItemsCountAsync();
        }

        public async Task<int> GetTotalTransactionsCountAsync()
        {
            return await _stockTransactionRepository.GetTotalTransactionsCountAsync();
        }

        #endregion

        #region Private Helper Methods

        private async Task<StockItem> GetStockItemEntityAsync(Guid stockItemId)
        {
            var stockItem = await _stockItemRepository.GetByIdAsync(stockItemId);
            if (stockItem == null)
                throw new ArgumentException($"Stock item with ID {stockItemId} not found", nameof(stockItemId));
            return stockItem;
        }

        private async Task ValidateProductExistsAsync(Guid productId)
        {
            var exists = await _productRepository.ExistsAsync(productId);
            if (!exists)
                throw new ArgumentException($"Product with ID {productId} not found", nameof(productId));
        }

        private async Task ValidateWarehouseExistsAsync(Guid warehouseId)
        {
            var exists = await _warehouseRepository.ExistsAsync(warehouseId);
            if (!exists)
                throw new ArgumentException($"Warehouse with ID {warehouseId} not found", nameof(warehouseId));
        }

        private async Task ValidateUserExistsAsync(Guid userId)
        {
            var exists = await _userRepository.ExistsAsync(userId);
            if (!exists)
                throw new ArgumentException($"User with ID {userId} not found", nameof(userId));
        }

        private async Task UpdateStockForTransaction(Guid productId, Guid warehouseId, int quantityChange)
        {
            var stockItems = await _stockItemRepository.GetByProductIdAsync(productId);
            var stockItem = stockItems.FirstOrDefault(si => si.WarehouseId == warehouseId);

            if (stockItem == null)
            {
                // Create new stock item if it doesn't exist
                stockItem = new StockItem(productId, warehouseId, Math.Max(0, quantityChange));
                await _stockItemRepository.AddAsync(stockItem);
            }
            else
            {
                // Update existing stock item
                if (quantityChange > 0)
                    stockItem.AddQuantity(quantityChange);
                else if (quantityChange < 0)
                    stockItem.RemoveQuantity(-quantityChange);

                await _stockItemRepository.UpdateAsync(stockItem);
            }
        }

        private async Task SetStockQuantity(Guid productId, Guid warehouseId, int quantity)
        {
            var stockItems = await _stockItemRepository.GetByProductIdAsync(productId);
            var stockItem = stockItems.FirstOrDefault(si => si.WarehouseId == warehouseId);

            if (stockItem == null)
            {
                stockItem = new StockItem(productId, warehouseId, quantity);
                await _stockItemRepository.AddAsync(stockItem);
            }
            else
            {
                stockItem.UpdateQuantity(quantity);
                await _stockItemRepository.UpdateAsync(stockItem);
            }
        }

        private async Task<StockItemDto> MapStockItemToDtoAsync(StockItem stockItem)
        {
            var product = await _productRepository.GetByIdAsync(stockItem.ProductId);
            var warehouse = await _warehouseRepository.GetByIdAsync(stockItem.WarehouseId);

            return new StockItemDto
            {
                Id = stockItem.Id,
                ProductId = stockItem.ProductId,
                WarehouseId = stockItem.WarehouseId,
                Quantity = stockItem.Quantity,
                IsLow = stockItem.IsLow(),
                IsOver = stockItem.IsOver(),
                ProductName = product?.Name,
                WarehouseName = warehouse?.Name,
                ProductUnit = product?.Unit
            };
        }

        private async Task<StockTransactionDto> MapTransactionToDtoAsync(StockTransaction transaction)
        {
            var product = await _productRepository.GetByIdAsync(transaction.ProductId);
            var warehouse = await _warehouseRepository.GetByIdAsync(transaction.WarehouseId);
            var user = await _userRepository.GetByIdAsync(transaction.UserId);

            return new StockTransactionDto
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                WarehouseId = transaction.WarehouseId,
                Type = transaction.Type,
                Quantity = transaction.Quantity,
                Reason = transaction.Reason,
                TransactionDate = transaction.TransactionDate,
                UserId = transaction.UserId,
                ProductName = product?.Name,
                WarehouseName = warehouse?.Name,
                Username = user?.Username,
                TypeName = transaction.Type.ToString()
            };
        }

        #endregion
    }
}
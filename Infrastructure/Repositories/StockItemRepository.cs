// Infrastructure/Repositories/StockItemRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StockItemRepository : IStockItemRepository
    {
        private readonly AppDbContext _context;

        public StockItemRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<StockItem?> GetByIdAsync(Guid id)
        {
            return await _context.StockItems
                .Include(si => si.Product)
                .Include(si => si.Warehouse)
                .FirstOrDefaultAsync(si => si.Id == id);
        }

        public async Task<IEnumerable<StockItem>> GetAllAsync()
        {
            return await _context.StockItems
                .Include(si => si.Product)
                .Include(si => si.Warehouse)
                .OrderBy(si => si.Product.Name)
                .ThenBy(si => si.Warehouse.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockItem>> GetByProductIdAsync(Guid productId)
        {
            return await _context.StockItems
                .Include(si => si.Product)
                .Include(si => si.Warehouse)
                .Where(si => si.ProductId == productId)
                .OrderBy(si => si.Warehouse.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockItem>> GetByWarehouseIdAsync(Guid warehouseId)
        {
            return await _context.StockItems
                .Include(si => si.Product)
                .Include(si => si.Warehouse)
                .Where(si => si.WarehouseId == warehouseId)
                .OrderBy(si => si.Product.Name)
                .ToListAsync();
        }

        public async Task<StockItem> AddAsync(StockItem stockItem)
        {
            if (stockItem == null)
                throw new ArgumentNullException(nameof(stockItem));

            await _context.StockItems.AddAsync(stockItem);
            await _context.SaveChangesAsync();
            return stockItem;
        }

        public async Task UpdateAsync(StockItem stockItem)
        {
            if (stockItem == null)
                throw new ArgumentNullException(nameof(stockItem));

            _context.StockItems.Update(stockItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem != null)
            {
                _context.StockItems.Remove(stockItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.StockItems.AnyAsync(si => si.Id == id);
        }

        public async Task<int> GetTotalStockItemsCountAsync()
        {
            return await _context.StockItems.CountAsync();
        }
    }
}
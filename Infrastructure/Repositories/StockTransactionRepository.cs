// Infrastructure/Repositories/StockTransactionRepository.cs
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
    public class StockTransactionRepository : IStockTransactionRepository
    {
        private readonly AppDbContext _context;

        public StockTransactionRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<StockTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Warehouse)
                .Include(st => st.User)
                .FirstOrDefaultAsync(st => st.Id == id);
        }

        public async Task<IEnumerable<StockTransaction>> GetAllAsync()
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Warehouse)
                .Include(st => st.User)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByProductIdAsync(Guid productId)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Warehouse)
                .Include(st => st.User)
                .Where(st => st.ProductId == productId)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByWarehouseIdAsync(Guid warehouseId)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Warehouse)
                .Include(st => st.User)
                .Where(st => st.WarehouseId == warehouseId)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Warehouse)
                .Include(st => st.User)
                .Where(st => st.UserId == userId)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByTypeAsync(TransactionType type)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Warehouse)
                .Include(st => st.User)
                .Where(st => st.Type == type)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<StockTransaction> AddAsync(StockTransaction stockTransaction)
        {
            if (stockTransaction == null)
                throw new ArgumentNullException(nameof(stockTransaction));

            await _context.StockTransactions.AddAsync(stockTransaction);
            await _context.SaveChangesAsync();
            return stockTransaction;
        }

        public async Task UpdateAsync(StockTransaction stockTransaction)
        {
            if (stockTransaction == null)
                throw new ArgumentNullException(nameof(stockTransaction));

            _context.StockTransactions.Update(stockTransaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var stockTransaction = await _context.StockTransactions.FindAsync(id);
            if (stockTransaction != null)
            {
                _context.StockTransactions.Remove(stockTransaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.StockTransactions.AnyAsync(st => st.Id == id);
        }

        public async Task<int> GetTotalTransactionsCountAsync()
        {
            return await _context.StockTransactions.CountAsync();
        }
    }
}
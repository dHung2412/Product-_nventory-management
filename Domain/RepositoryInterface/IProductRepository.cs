// Domain/Interfaces/IProductRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProductRepository
    {
        // Basic query operations
        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);

        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);

        // Existence checks
        Task<bool> ExistsAsync(Guid id);
        
        // Statistics
        Task<int> GetTotalProductsCountAsync();
    }
}
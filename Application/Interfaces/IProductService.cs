// Application/Interfaces/IProductService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductService
    {
        // Basic CRUD operations
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category);
        Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto updateProductDto);
        Task<bool> DeleteAsync(Guid id);

        // Validation methods
        Task<bool> ExistsAsync(Guid id);

        // Statistics
        Task<int> GetTotalProductsCountAsync();

        // Business logic methods
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    }
}
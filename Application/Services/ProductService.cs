// Application/Services/ProductService.cs
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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or empty", nameof(category));

            var products = await _productRepository.GetByCategoryAsync(category);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
        {
            if (createProductDto == null)
                throw new ArgumentNullException(nameof(createProductDto));

            var product = new Product(
                createProductDto.Name,
                createProductDto.Description,
                createProductDto.Unit,
                createProductDto.Price,
                createProductDto.Category
            );

            var createdProduct = await _productRepository.AddAsync(product);
            return MapToDto(createdProduct);
        }

        public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
                throw new ArgumentNullException(nameof(updateProductDto));

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException($"Product with ID {id} not found", nameof(id));

            product.Update(
                updateProductDto.Name,
                updateProductDto.Description,
                updateProductDto.Unit,
                updateProductDto.Price,
                updateProductDto.Category
            );

            await _productRepository.UpdateAsync(product);
            return MapToDto(product);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
                return false;

            await _productRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _productRepository.ExistsAsync(id);
        }

        public async Task<int> GetTotalProductsCountAsync()
        {
            return await _productRepository.GetTotalProductsCountAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => p.Category).Distinct().OrderBy(c => c);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var products = await _productRepository.GetAllAsync();
            var filteredProducts = products.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            );

            return filteredProducts.Select(MapToDto);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Unit = product.Unit,
                Price = product.Price,
                Category = product.Category
            };
        }
    }
}
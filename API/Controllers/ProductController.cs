// API/Controllers/ProductController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, new { message = "Error retrieving products" });
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }
                
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
                return StatusCode(500, new { message = "Error retrieving product" });
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="category">Product category</param>
        /// <returns>List of products in the specified category</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCategory(string category)
        {
            try
            {
                var products = await _productService.GetByCategoryAsync(category);
                return Ok(products);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for category {Category}", category);
                return StatusCode(500, new { message = "Error retrieving products" });
            }
        }

        /// <summary>
        /// Search products by name, description, or category
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching products</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term {SearchTerm}", searchTerm);
                return StatusCode(500, new { message = "Error searching products" });
            }
        }

        /// <summary>
        /// Get all product categories
        /// </summary>
        /// <returns>List of unique categories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _productService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product categories");
                return StatusCode(500, new { message = "Error retrieving categories" });
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="createProductDto">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _productService.CreateAsync(createProductDto);
                
                _logger.LogInformation("Product {ProductName} created successfully with ID {ProductId}", 
                    product.Name, product.Id);
                
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductName}", createProductDto?.Name);
                return StatusCode(500, new { message = "Error creating product" });
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateProductDto">Product update data</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _productService.UpdateAsync(id, updateProductDto);
                
                _logger.LogInformation("Product with ID {ProductId} updated successfully", id);
                
                return Ok(product);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                return StatusCode(500, new { message = "Error updating product" });
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _productService.DeleteAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }
                
                _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode(500, new { message = "Error deleting product" });
            }
        }

        /// <summary>
        /// Get product statistics
        /// </summary>
        /// <returns>Product statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalCount = await _productService.GetTotalProductsCountAsync();
                var categories = await _productService.GetCategoriesAsync();
                
                var statistics = new
                {
                    TotalProducts = totalCount,
                    TotalCategories = categories.Count(),
                    Categories = categories
                };
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product statistics");
                return StatusCode(500, new { message = "Error retrieving statistics" });
            }
        }

        /// <summary>
        /// Check if product exists
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Existence status</returns>
        [HttpHead("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Exists(Guid id)
        {
            try
            {
                var exists = await _productService.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists with ID {ProductId}", id);
                return StatusCode(500);
            }
        }
    }
}
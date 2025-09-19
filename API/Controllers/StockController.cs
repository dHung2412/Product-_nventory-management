// API/Controllers/StockController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
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
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ILogger<StockController> _logger;

        public StockController(IStockService stockService, ILogger<StockController> logger)
        {
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Stock Items

        /// <summary>
        /// Get all stock items
        /// </summary>
        /// <returns>List of all stock items</returns>
        [HttpGet("items")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStockItems()
        {
            try
            {
                var stockItems = await _stockService.GetAllStockItemsAsync();
                return Ok(stockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all stock items");
                return StatusCode(500, new { message = "Error retrieving stock items" });
            }
        }

        /// <summary>
        /// Get stock item by ID
        /// </summary>
        /// <param name="id">Stock item ID</param>
        /// <returns>Stock item details</returns>
        [HttpGet("items/{id:guid}")]
        [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStockItemById(Guid id)
        {
            try
            {
                var stockItem = await _stockService.GetStockItemByIdAsync(id);
                
                if (stockItem == null)
                {
                    return NotFound(new { message = $"Stock item with ID {id} not found" });
                }
                
                return Ok(stockItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock item with ID {StockItemId}", id);
                return StatusCode(500, new { message = "Error retrieving stock item" });
            }
        }

        /// <summary>
        /// Get stock items by product ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of stock items for the product</returns>
        [HttpGet("items/product/{productId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStockItemsByProduct(Guid productId)
        {
            try
            {
                var stockItems = await _stockService.GetStockItemsByProductIdAsync(productId);
                return Ok(stockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock items for product {ProductId}", productId);
                return StatusCode(500, new { message = "Error retrieving stock items" });
            }
        }

        /// <summary>
        /// Get stock items by warehouse ID
        /// </summary>
        /// <param name="warehouseId">Warehouse ID</param>
        /// <returns>List of stock items in the warehouse</returns>
        [HttpGet("items/warehouse/{warehouseId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStockItemsByWarehouse(Guid warehouseId)
        {
            try
            {
                var stockItems = await _stockService.GetStockItemsByWarehouseIdAsync(warehouseId);
                return Ok(stockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock items for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "Error retrieving stock items" });
            }
        }

        /// <summary>
        /// Create a new stock item
        /// </summary>
        /// <param name="createStockItemDto">Stock item creation data</param>
        /// <returns>Created stock item</returns>
        [HttpPost("items")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateStockItem([FromBody] CreateStockItemDto createStockItemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var stockItem = await _stockService.CreateStockItemAsync(createStockItemDto);
                
                _logger.LogInformation("Stock item created successfully with ID {StockItemId}", stockItem.Id);
                
                return CreatedAtAction(nameof(GetStockItemById), new { id = stockItem.Id }, stockItem);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock item");
                return StatusCode(500, new { message = "Error creating stock item" });
            }
        }

        /// <summary>
        /// Update stock item quantity
        /// </summary>
        /// <param name="id">Stock item ID</param>
        /// <param name="updateDto">Quantity update data</param>
        /// <returns>Updated stock item</returns>
        [HttpPut("items/{id:guid}/quantity")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStockQuantity(Guid id, [FromBody] UpdateStockItemQuantityDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var stockItem = await _stockService.UpdateStockQuantityAsync(id, updateDto);
                
                _logger.LogInformation("Stock item {StockItemId} quantity updated to {Quantity}", id, updateDto.Quantity);
                
                return Ok(stockItem);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock item quantity {StockItemId}", id);
                return StatusCode(500, new { message = "Error updating stock quantity" });
            }
        }

        /// <summary>
        /// Add stock to an existing stock item
        /// </summary>
        /// <param name="id">Stock item ID</param>
        /// <param name="addStockDto">Stock addition data</param>
        /// <returns>Updated stock item</returns>
        [HttpPost("items/{id:guid}/add")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddStock(Guid id, [FromBody] AddStockDto addStockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var stockItem = await _stockService.AddStockAsync(id, addStockDto);
                
                _logger.LogInformation("Added {Amount} to stock item {StockItemId}", addStockDto.Amount, id);
                
                return Ok(stockItem);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock to item {StockItemId}", id);
                return StatusCode(500, new { message = "Error adding stock" });
            }
        }

        /// <summary>
        /// Remove stock from an existing stock item
        /// </summary>
        /// <param name="id">Stock item ID</param>
        /// <param name="removeStockDto">Stock removal data</param>
        /// <returns>Updated stock item</returns>
        [HttpPost("items/{id:guid}/remove")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveStock(Guid id, [FromBody] RemoveStockDto removeStockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var stockItem = await _stockService.RemoveStockAsync(id, removeStockDto);
                
                _logger.LogInformation("Removed {Amount} from stock item {StockItemId}", removeStockDto.Amount, id);
                
                return Ok(stockItem);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stock from item {StockItemId}", id);
                return StatusCode(500, new { message = "Error removing stock" });
            }
        }

        /// <summary>
        /// Delete a stock item
        /// </summary>
        /// <param name="id">Stock item ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("items/{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteStockItem(Guid id)
        {
            try
            {
                var result = await _stockService.DeleteStockItemAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = $"Stock item with ID {id} not found" });
                }
                
                _logger.LogInformation("Stock item {StockItemId} deleted successfully", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stock item {StockItemId}", id);
                return StatusCode(500, new { message = "Error deleting stock item" });
            }
        }

        #endregion

        #region Stock Transactions

        /// <summary>
        /// Get all stock transactions
        /// </summary>
        /// <returns>List of all stock transactions</returns>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(IEnumerable<StockTransactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions = await _stockService.GetAllTransactionsAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all stock transactions");
                return StatusCode(500, new { message = "Error retrieving transactions" });
            }
        }

        /// <summary>
        /// Get stock transaction by ID
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <returns>Transaction details</returns>
        [HttpGet("transactions/{id:guid}")]
        [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            try
            {
                var transaction = await _stockService.GetTransactionByIdAsync(id);
                
                if (transaction == null)
                {
                    return NotFound(new { message = $"Transaction with ID {id} not found" });
                }
                
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction with ID {TransactionId}", id);
                return StatusCode(500, new { message = "Error retrieving transaction" });
            }
        }

        /// <summary>
        /// Get transactions by product ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of transactions for the product</returns>
        [HttpGet("transactions/product/{productId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<StockTransactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByProduct(Guid productId)
        {
            try
            {
                var transactions = await _stockService.GetTransactionsByProductIdAsync(productId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for product {ProductId}", productId);
                return StatusCode(500, new { message = "Error retrieving transactions" });
            }
        }

        /// <summary>
        /// Get transactions by warehouse ID
        /// </summary>
        /// <param name="warehouseId">Warehouse ID</param>
        /// <returns>List of transactions for the warehouse</returns>
        [HttpGet("transactions/warehouse/{warehouseId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<StockTransactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByWarehouse(Guid warehouseId)
        {
            try
            {
                var transactions = await _stockService.GetTransactionsByWarehouseIdAsync(warehouseId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "Error retrieving transactions" });
            }
        }

        /// <summary>
        /// Get transactions by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of transactions by the user</returns>
        [HttpGet("transactions/user/{userId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<StockTransactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByUser(Guid userId)
        {
            try
            {
                var transactions = await _stockService.GetTransactionsByUserIdAsync(userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving transactions" });
            }
        }

        /// <summary>
        /// Get transactions by type
        /// </summary>
        /// <param name="type">Transaction type</param>
        /// <returns>List of transactions of the specified type</returns>
        [HttpGet("transactions/type/{type}")]
        [ProducesResponseType(typeof(IEnumerable<StockTransactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByType(TransactionType type)
        {
            try
            {
                var transactions = await _stockService.GetTransactionsByTypeAsync(type);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for type {TransactionType}", type);
                return StatusCode(500, new { message = "Error retrieving transactions" });
            }
        }

        /// <summary>
        /// Get transactions by date range
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>List of transactions within the date range</returns>
        [HttpGet("transactions/date-range")]
        [ProducesResponseType(typeof(IEnumerable<StockTransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionsByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                {
                    return BadRequest(new { message = "From date cannot be greater than to date" });
                }

                var transactions = await _stockService.GetTransactionsByDateRangeAsync(fromDate, toDate);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for date range {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new { message = "Error retrieving transactions" });
            }
        }

        #endregion

        #region Business Operations

        /// <summary>
        /// Import stock
        /// </summary>
        /// <param name="importStockDto">Import stock data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("import")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportStock([FromBody] ImportStockDto importStockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Set current user ID if not provided
                if (importStockDto.UserId == Guid.Empty)
                {
                    var userIdClaim = User.FindFirst("userId")?.Value;
                    if (Guid.TryParse(userIdClaim, out var userId))
                    {
                        importStockDto.UserId = userId;
                    }
                    else
                    {
                        return BadRequest(new { message = "User ID not found in token" });
                    }
                }

                var transaction = await _stockService.ImportStockAsync(importStockDto);
                
                _logger.LogInformation("Stock imported: {Quantity} units of product {ProductId} to warehouse {WarehouseId}", 
                    importStockDto.Quantity, importStockDto.ProductId, importStockDto.WarehouseId);
                
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing stock");
                return StatusCode(500, new { message = "Error importing stock" });
            }
        }

        /// <summary>
        /// Export stock
        /// </summary>
        /// <param name="exportStockDto">Export stock data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("export")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportStock([FromBody] ExportStockDto exportStockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Set current user ID if not provided
                if (exportStockDto.UserId == Guid.Empty)
                {
                    var userIdClaim = User.FindFirst("userId")?.Value;
                    if (Guid.TryParse(userIdClaim, out var userId))
                    {
                        exportStockDto.UserId = userId;
                    }
                    else
                    {
                        return BadRequest(new { message = "User ID not found in token" });
                    }
                }

                var transaction = await _stockService.ExportStockAsync(exportStockDto);
                
                _logger.LogInformation("Stock exported: {Quantity} units of product {ProductId} from warehouse {WarehouseId}", 
                    exportStockDto.Quantity, exportStockDto.ProductId, exportStockDto.WarehouseId);
                
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting stock");
                return StatusCode(500, new { message = "Error exporting stock" });
            }
        }

        /// <summary>
        /// Adjust stock
        /// </summary>
        /// <param name="adjustStockDto">Adjust stock data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("adjust")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto adjustStockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Set current user ID if not provided
                if (adjustStockDto.UserId == Guid.Empty)
                {
                    var userIdClaim = User.FindFirst("userId")?.Value;
                    if (Guid.TryParse(userIdClaim, out var userId))
                    {
                        adjustStockDto.UserId = userId;
                    }
                    else
                    {
                        return BadRequest(new { message = "User ID not found in token" });
                    }
                }

                var transaction = await _stockService.AdjustStockAsync(adjustStockDto);
                
                _logger.LogInformation("Stock adjusted: Product {ProductId} in warehouse {WarehouseId} to {Quantity} units", 
                    adjustStockDto.ProductId, adjustStockDto.WarehouseId, adjustStockDto.Quantity);
                
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting stock");
                return StatusCode(500, new { message = "Error adjusting stock" });
            }
        }

        #endregion

        #region Analytics and Reports

        /// <summary>
        /// Get low stock items
        /// </summary>
        /// <returns>List of low stock items</returns>
        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLowStockItems()
        {
            try
            {
                var lowStockItems = await _stockService.GetLowStockItemsAsync();
                return Ok(lowStockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock items");
                return StatusCode(500, new { message = "Error retrieving low stock items" });
            }
        }

        /// <summary>
        /// Get over stock items
        /// </summary>
        /// <returns>List of over stock items</returns>
        [HttpGet("over-stock")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverStockItems()
        {
            try
            {
                var overStockItems = await _stockService.GetOverStockItemsAsync();
                return Ok(overStockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving over stock items");
                return StatusCode(500, new { message = "Error retrieving over stock items" });
            }
        }

        /// <summary>
        /// Get stock summary by warehouse
        /// </summary>
        /// <param name="warehouseId">Warehouse ID</param>
        /// <returns>Stock summary for the warehouse</returns>
        [HttpGet("summary/warehouse/{warehouseId:guid}")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStockSummaryByWarehouse(Guid warehouseId)
        {
            try
            {
                var summary = await _stockService.GetStockSummaryByWarehouseAsync(warehouseId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock summary for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "Error retrieving stock summary" });
            }
        }

        /// <summary>
        /// Check available stock
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="warehouseId">Warehouse ID</param>
        /// <returns>Available stock quantity</returns>
        [HttpGet("available")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableStock([FromQuery] Guid productId, [FromQuery] Guid warehouseId)
        {
            try
            {
                if (productId == Guid.Empty || warehouseId == Guid.Empty)
                {
                    return BadRequest(new { message = "Product ID and Warehouse ID are required" });
                }

                var availableQuantity = await _stockService.GetAvailableStockAsync(productId, warehouseId);
                
                return Ok(new { 
                    productId, 
                    warehouseId, 
                    availableQuantity 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking available stock for product {ProductId} in warehouse {WarehouseId}", 
                    productId, warehouseId);
                return StatusCode(500, new { message = "Error checking available stock" });  
            }
        }

        /// <summary>
        /// Get stock statistics
        /// </summary>
        /// <returns>Stock statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalStockItems = await _stockService.GetTotalStockItemsCountAsync();
                var totalTransactions = await _stockService.GetTotalTransactionsCountAsync();
                var lowStockItems = await _stockService.GetLowStockItemsAsync();
                var overStockItems = await _stockService.GetOverStockItemsAsync();

                var statistics = new
                {
                    TotalStockItems = totalStockItems,
                    TotalTransactions = totalTransactions,
                    LowStockItemsCount = lowStockItems.Count(),
                    OverStockItemsCount = overStockItems.Count()
                };
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock statistics");
                return StatusCode(500, new { message = "Error retrieving statistics" });
            }
        }

        #endregion
    }
}
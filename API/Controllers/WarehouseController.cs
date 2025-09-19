// API/Controllers/WarehouseController.cs
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
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
        {
            _warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all warehouses
        /// </summary>
        /// <returns>List of all warehouses</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var warehouses = await _warehouseService.GetAllAsync();
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all warehouses");
                return StatusCode(500, new { message = "Error retrieving warehouses" });
            }
        }

        /// <summary>
        /// Get warehouse by ID
        /// </summary>
        /// <param name="id">Warehouse ID</param>
        /// <returns>Warehouse details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var warehouse = await _warehouseService.GetByIdAsync(id);
                
                if (warehouse == null)
                {
                    return NotFound(new { message = $"Warehouse with ID {id} not found" });
                }
                
                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "Error retrieving warehouse" });
            }
        }

        /// <summary>
        /// Get stock items in a specific warehouse
        /// </summary>
        /// <param name="id">Warehouse ID</param>
        /// <returns>List of stock items in the warehouse</returns>
        [HttpGet("{id:guid}/stock")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWarehouseStock(Guid id)
        {
            try
            {
                var exists = await _warehouseService.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound(new { message = $"Warehouse with ID {id} not found" });
                }

                var stockItems = await _warehouseService.GetWarehouseStockAsync(id);
                return Ok(stockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock for warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "Error retrieving warehouse stock" });
            }
        }

        /// <summary>
        /// Create a new warehouse
        /// </summary>
        /// <param name="createWarehouseDto">Warehouse creation data</param>
        /// <returns>Created warehouse</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseDto createWarehouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var warehouse = await _warehouseService.CreateAsync(createWarehouseDto);
                
                _logger.LogInformation("Warehouse {WarehouseName} created successfully with ID {WarehouseId}", 
                    warehouse.Name, warehouse.Id);
                
                return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse {WarehouseName}", createWarehouseDto?.Name);
                return StatusCode(500, new { message = "Error creating warehouse" });
            }
        }

        /// <summary>
        /// Update an existing warehouse
        /// </summary>
        /// <param name="id">Warehouse ID</param>
        /// <param name="updateWarehouseDto">Warehouse update data</param>
        /// <returns>Updated warehouse</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto updateWarehouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var warehouse = await _warehouseService.UpdateAsync(id, updateWarehouseDto);
                
                _logger.LogInformation("Warehouse with ID {WarehouseId} updated successfully", id);
                
                return Ok(warehouse);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "Error updating warehouse" });
            }
        }

        /// <summary>
        /// Delete a warehouse
        /// </summary>
        /// <param name="id">Warehouse ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var canDelete = await _warehouseService.CanDeleteWarehouseAsync(id);
                if (!canDelete)
                {
                    return BadRequest(new { message = "Cannot delete warehouse that contains stock items" });
                }

                var result = await _warehouseService.DeleteAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = $"Warehouse with ID {id} not found" });
                }
                
                _logger.LogInformation("Warehouse with ID {WarehouseId} deleted successfully", id);
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "Error deleting warehouse" });
            }
        }

        /// <summary>
        /// Get warehouse statistics
        /// </summary>
        /// <returns>Warehouse statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalCount = await _warehouseService.GetTotalWarehousesCountAsync();
                
                var statistics = new
                {
                    TotalWarehouses = totalCount
                };
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouse statistics");
                return StatusCode(500, new { message = "Error retrieving statistics" });
            }
        }

        /// <summary>
        /// Check if warehouse can be deleted
        /// </summary>
        /// <param name="id">Warehouse ID</param>
        /// <returns>Deletion possibility status</returns>
        [HttpGet("{id:guid}/can-delete")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CanDelete(Guid id)
        {
            try
            {
                var exists = await _warehouseService.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound(new { message = $"Warehouse with ID {id} not found" });
                }

                var canDelete = await _warehouseService.CanDeleteWarehouseAsync(id);
                
                return Ok(new { canDelete, message = canDelete ? "Warehouse can be deleted" : "Warehouse contains stock items and cannot be deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if warehouse can be deleted with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "Error checking deletion status" });
            }
        }

        /// <summary>
        /// Check if warehouse exists
        /// </summary>
        /// <param name="id">Warehouse ID</param>
        /// <returns>Existence status</returns>
        [HttpHead("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Exists(Guid id)
        {
            try
            {
                var exists = await _warehouseService.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if warehouse exists with ID {WarehouseId}", id);
                return StatusCode(500);
            }
        }
    }
}
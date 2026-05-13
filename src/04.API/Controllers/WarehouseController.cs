using Inventory.Application.DTOs.Warehouses;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.WebApi.ResourceBases;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly IAuthorizationService _authorizationService;

    public WarehouseController(IWarehouseService warehouseService, IAuthorizationService authorizationService)
    {
        _warehouseService = warehouseService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Authorize(Roles = "Super_Admin")]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _warehouseService.GetAllAsync(cancellationToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "WAREHOUSE_VIEW")]
    public async Task<ActionResult<WarehouseDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _warehouseService.GetByIdAsync(id, cancellationToken); 
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        var warehouse = result.Data;
        // Resource-based
        var isAuthorized = await _authorizationService
            .AuthorizeAsync(User, warehouse, new WarehouseScopeRequirement());
        if (!isAuthorized.Succeeded) return Forbid();

        return Ok(warehouse);
    }

    [HttpPost]
    [Authorize(Roles = "Super_Admin")]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseDto createDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _warehouseService.CreateAsync(createDto, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "WAREHOUSE_EDIT")]
    public async Task<ActionResult<WarehouseDto>> Update(int id, [FromBody] UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _warehouseService.UpdateAsync(id, updateDto, cancellationToken);
        
        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(result.ErrorMessage);
            }
            return BadRequest(result.ErrorMessage);
        }

        var warehouse = result.Data;
        // Resource-based
        var isAuthorized = await _authorizationService
            .AuthorizeAsync(User, warehouse, new WarehouseScopeRequirement());
        if (!isAuthorized.Succeeded) return Forbid();

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Super_Admin")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await _warehouseService.DeleteAsync(id, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }
        return NoContent();
    }

    [HttpGet("{id}/exists")]
    [Authorize(Policy = "WAREHOUSE_EDIT")]
    public async Task<ActionResult<bool>> Exists(int id, CancellationToken cancellationToken = default)
    {
        var result = await _warehouseService.ExistsAsync(id, cancellationToken);
        return Ok(result.Data);
    }
}


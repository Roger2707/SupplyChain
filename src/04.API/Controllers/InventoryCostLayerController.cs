using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryCostLayerController : ControllerBase
{
    private readonly IInventoryCostLayerService _inventoryCostLayerService;

    public InventoryCostLayerController(IInventoryCostLayerService inventoryCostLayerService)
    {
        _inventoryCostLayerService = inventoryCostLayerService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InventoryCostLayer>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _inventoryCostLayerService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<InventoryCostLayer>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _inventoryCostLayerService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Data);
    }
}


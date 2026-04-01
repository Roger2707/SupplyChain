using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryLedgerController : ControllerBase
{
    private readonly IInventoryLedgerService _inventoryLedgerService;

    public InventoryLedgerController(IInventoryLedgerService inventoryLedgerService)
    {
        _inventoryLedgerService = inventoryLedgerService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InventoryLedger>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _inventoryLedgerService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<InventoryLedger>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _inventoryLedgerService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Data);
    }
}


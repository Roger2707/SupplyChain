using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.StockTransfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransferController : ControllerBase
{
    private readonly IStockTransferService _stockTransferService;

    public StockTransferController(IStockTransferService stockTransferService)
    {
        _stockTransferService = stockTransferService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<StockTransfer>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _stockTransferService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<StockTransfer>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _stockTransferService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Data);
    }
}


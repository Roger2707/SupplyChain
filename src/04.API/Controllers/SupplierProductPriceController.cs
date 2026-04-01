using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Suppliers;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplierProductPriceController : ControllerBase
{
    private readonly ISupplierProductPriceService _supplierProductPriceService;

    public SupplierProductPriceController(ISupplierProductPriceService supplierProductPriceService)
    {
        _supplierProductPriceService = supplierProductPriceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierProductPrice>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _supplierProductPriceService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierProductPrice>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _supplierProductPriceService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Data);
    }
}


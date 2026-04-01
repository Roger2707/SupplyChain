using Inventory.Application.DTOs.GoodsReceipts;
using Inventory.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoodsReceiptController : ControllerBase
{
    private readonly IGoodsReceiptService _goodsReceiptService;

    public GoodsReceiptController(IGoodsReceiptService goodsReceiptService)
    {
        _goodsReceiptService = goodsReceiptService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GoodsReceiptDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _goodsReceiptService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GoodsReceiptDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _goodsReceiptService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<GoodsReceiptDto>> Create([FromBody] CreateGoodsReceiptDto createGoodsReceipt, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _goodsReceiptService.CreateAsync(createGoodsReceipt, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GoodsReceiptDto>> Update(int id, [FromBody] UpdateGoodsReceiptDto updateDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _goodsReceiptService.UpdateAsync(id, updateDto, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(result.ErrorMessage);
            }
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await _goodsReceiptService.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }
        return NoContent();
    }

    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id, CancellationToken cancellationToken = default)
    {
        var result = await _goodsReceiptService.ExistAsync(id, cancellationToken);
        return Ok(result.Data);
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult> Post(int id, CancellationToken cancellationToken = default)
    {
        var result = await _goodsReceiptService.PostAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }
        return NoContent();
    }
}


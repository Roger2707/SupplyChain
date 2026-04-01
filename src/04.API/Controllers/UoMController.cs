using Inventory.Application.DTOs.UoMs;
using Inventory.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UoMController : ControllerBase
{
    private readonly IUoMService _uomService;

    public UoMController(IUoMService categoryService)
    {
        _uomService = categoryService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UoMDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _uomService.GetAllUoMsAsync(cancellationToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UoMDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _uomService.GetUoMByIdAsync(id, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<UoMDto>> Create([FromBody] UoMCreateDto createDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _uomService.CreateUoMAsync(createDto, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<UoMDto>> Update(int id, [FromBody] UoMUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _uomService.UpdateUoMAsync(id, updateDto, cancellationToken);
        
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
    [Authorize]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await _uomService.DeleteUoMAsync(id, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }
        return NoContent();
    }

    [HttpGet("{id}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> Exists(int id, CancellationToken cancellationToken = default)
    {
        var result = await _uomService.ExistsAsync(id, cancellationToken);
        return Ok(result.Data);
    }
}


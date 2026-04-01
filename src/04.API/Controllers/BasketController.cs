using ECommerce.Application.DTOs.Baskets;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }


        [HttpGet]
        public async Task<ActionResult<BasketDto>> GetById(CancellationToken cancellationToken = default)
        {

            var result = await _basketService.GetByUserIdAsync(cancellationToken);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpPost("increase")]
        public async Task<ActionResult<BasketDto>> Increase([FromBody] UpsertBasketItem upsertBasketDto, CancellationToken cancellationToken = default)
        {
            var result = await _basketService.IncreaseQuantity(upsertBasketDto, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPost("decrease")]
        public async Task<ActionResult<BasketDto>> Decrease([FromBody] UpsertBasketItem upsertBasketDto, CancellationToken cancellationToken = default)
        {
            var result = await _basketService.DecreaseQuantity(upsertBasketDto, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _basketService.DeleteAsync(id, cancellationToken);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return NoContent();
        }

        [HttpPost("clear")]
        public async Task<ActionResult<BasketDto>> Clear(CancellationToken cancellationToken = default)
        {
            var result = await _basketService.ClearBasketAsync(cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }
    }
}

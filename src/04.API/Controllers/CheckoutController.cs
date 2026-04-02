using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        [HttpPost("check-out")]
        public async Task<ActionResult<CheckoutResponseDto>> CheckoutAsync([FromBody] OrderCreateDto orderCreateDto, CancellationToken cancellationToken = default)
        {
            var result = await _checkoutService.CheckoutAsync(orderCreateDto, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
            return Ok(result.Data);
        }
    }
}

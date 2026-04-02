using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _orderService.GetOrderAsync(id, cancellationToken);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<OrderDto>>> GetOrdersByUserId(CancellationToken cancellationToken = default)
        {
            var result = await _orderService.GetOrdersByUserAsync(cancellationToken);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }
    }
}

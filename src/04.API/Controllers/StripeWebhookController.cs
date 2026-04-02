using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Ultilities;
using Stripe;

namespace SupplyChain.WebApi.Controllers
{
    [ApiController]
    [Route("api/webhook/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;
        private readonly string _webhookSecret;

        public const string PaymentIntentSucceeded = "payment_intent.succeeded";
        public const string PaymentIntentFailed = "payment_intent.payment_failed";

        public StripeWebhookController(IConfiguration configuration, IOrderService orderService)
        {
            _configuration = configuration;
            _orderService = orderService;
            _webhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> HandleWebhookAsync(CancellationToken cancellationToken = default)
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                if (stripeEvent.Type == PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    if (paymentIntent.Metadata.TryGetValue("OrderId", out var orderIdStr))
                    {
                        var orderId = CF.GetInt(orderIdStr);
                        await _orderService.ProcessCheckoutSuccessAsync(orderId, paymentIntent.Id, cancellationToken);
                    }
                }

                else if (stripeEvent.Type == PaymentIntentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    Console.WriteLine($"Payment failed for Intent: {paymentIntent.Id}");
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}

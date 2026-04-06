using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Services;
using ECommerce.Infrastructure.Queries;
using ECommerce.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Repositories;

namespace ECommerce.Infrastructure.Services
{
    public static class ServicesRegister
    {
        public static void AddEcommerceServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IStripeService, StripeService>();
            services.AddScoped<ICheckoutService, CheckoutService>();
            services.AddScoped<IInventoryAdapterService, InventoryAdapterService>();

            // Register Dapper query services (read-model)
            services.AddScoped<IDapperExecutor, DapperExecutor>();
        }
    }
}

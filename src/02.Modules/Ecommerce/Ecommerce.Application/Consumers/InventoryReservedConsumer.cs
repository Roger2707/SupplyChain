using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities.Orders;
using MassTransit;
using SharedKernel.Contracts;

namespace ECommerce.Application.Consumers
{
    public class InventoryReservedConsumer : IConsumer<InventoryReserved>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryAdapterService _inventoryAdapter;

        public InventoryReservedConsumer(IUnitOfWork unitOfWork, IInventoryAdapterService inventoryAdapter)
        {
            _unitOfWork = unitOfWork;
            _inventoryAdapter = inventoryAdapter;
        }

        public async Task Consume(ConsumeContext<InventoryReserved> context)
        {
            // track order
            var order = await _unitOfWork.OrderRepository.GetWithLinesAsync(context.Message.OrderId);
            if (order == null) return;

            // 1. Get (Selling Price) to calc Total
            var productIds = context.Message.Details.Select(d => d.ProductId).Distinct().ToList();
            var pricesDic = await _inventoryAdapter.GetProductsSellingPrice(productIds, context.CancellationToken);

            // 2. Create OrderItems from splited data of Inventory module
            order.Items = context.Message.Details.Select(d => new OrderItem
            {
                OrderId = order.Id,
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                Quantity = d.ReservedQty,
                UnitCost = d.UnitCost, 
                UnitPrice = pricesDic.TryGetValue(d.ProductId, out var p) ? p.SellingPrice : 0
            }).ToList();

            // 3. Update TotalAmount
            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}

using ECommerce.Domain.Entities.Enums;
using MassTransit;
using SharedKernel.Contracts;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Application.Consumers
{
    public class OrderPaymentTimeoutConsumer : IConsumer<OrderPaymentTimeoutCheck>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryAdapterService _inventoryService;

        public OrderPaymentTimeoutConsumer(IUnitOfWork unitOfWork, IInventoryAdapterService inventoryService)
        {
            _unitOfWork = unitOfWork;
            _inventoryService = inventoryService;
        }

        public async Task Consume(ConsumeContext<OrderPaymentTimeoutCheck> context)
        {

            var order = await _unitOfWork.OrderRepository.GetByIdAsync(context.Message.OrderId, CancellationToken.None);

            // Handle case: Order not found (maybe already deleted or invalid OrderId)
            if (order != null && order.OrderStatus == OrderStatus.Pending)
            {
                order.OrderStatus = OrderStatus.Cancelled; // 2

                // Cancel reserved stock in inventory (Release reserved stock back to available)
                await _inventoryService.ReleaseReserveQtyInLayers(context.Message.OrderId);

                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}

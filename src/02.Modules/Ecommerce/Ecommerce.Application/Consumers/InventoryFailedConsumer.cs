using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities.Enums;
using MassTransit;
using SharedKernel.Contracts;

namespace ECommerce.Application.Consumers
{
    public class InventoryFailedConsumer : IConsumer<InventoryReservationFailed>
    {
        private readonly IUnitOfWork _unitOfWork;
        public InventoryFailedConsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<InventoryReservationFailed> context)
        {
            var order = await _unitOfWork.OrderRepository.GetWithLinesAsync(context.Message.OrderId);        
            if (order != null)
            {
                order.OrderStatus = OrderStatus.Cancelled;
                order.CancelReason = context.Message.Reason;
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}

using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using MassTransit;
using SharedKernel.Contracts;
using SharedKernel.DTOs;

namespace Inventory.Application.Consumers
{
    public class ReserveInventoryConsumer : IConsumer<OrderCreated>
    {
        private readonly IInventoryService _inventoryService;
        private readonly IUnitOfWork _unitOfWork;

        public ReserveInventoryConsumer(IInventoryService inventoryService, IUnitOfWork unitOfWork)
        {
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var reservesDto = await _inventoryService.ReserveFIFOAsync(context.Message.Items.Select(i => new FIFOItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    NeccessaryQty = i.NeccessaryQty,
                    SourceId = context.Message.OrderId,
                    SourceType = "Order"
                }).ToList(), context.CancellationToken);

                var reservedDetails = reservesDto.Select(r => new ReservedItemDetail(
                    r.ProductId,
                    r.ProductName,
                    r.ReservedQty,
                    r.UnitCost
                )).ToList();

                await context.Publish(new InventoryReserved(context.Message.OrderId, reservedDetails));

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                // Send Message Failure to Ecommerce in order to Rollback Order
                await context.Publish(new InventoryReservationFailed(context.Message.OrderId, ex.Message));
            }
        }
    }
}

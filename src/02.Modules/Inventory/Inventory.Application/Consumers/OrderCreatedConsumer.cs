using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Contracts;
using SharedKernel.DTOs;

namespace Inventory.Application.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly IInventoryService _inventoryService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreatedConsumer(IInventoryService inventoryService, IUnitOfWork unitOfWork)
        {
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var isExisted = await _unitOfWork.InventoryReservationRepository
                .ExistsAsync(r => r.SourceId == context.Message.OrderId);

            if (isExisted) return;
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
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                // THROW: In order to let MassTransit Retry (error temp)
                throw;
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

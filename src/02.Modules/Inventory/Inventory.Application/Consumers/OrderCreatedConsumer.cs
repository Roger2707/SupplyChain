using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using MassTransit;
using Microsoft.Data.SqlClient;
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
                .ExistsAsync(r => r.SourceId == context.Message.OrderId && r.SourceType == "Order");

            if (isExisted) return;
            try
            {
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

                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (DbUpdateException ex) when (IsDuplicateReservation(ex))
            {
                // Another concurrent consumer already reserved this order.
                // Treat as idempotent success and avoid publishing failure.
                return;
            }
            catch (Exception ex)
            {
                // Send Message Failure to Ecommerce in order to Rollback Order
                await context.Publish(new InventoryReservationFailed(context.Message.OrderId, ex.Message));
            }
        }

        private static bool IsDuplicateReservation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx
                && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}

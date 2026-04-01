using Inventory.Application.DTOs.Delivery;
using Inventory.Application.Interfaces.Generators;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Accounts;
using Inventory.Domain.Entities.Delivery;
using Inventory.Domain.Entities.Inventory;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Inventory.Application.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeliveryGenerator _deliveryGenerator;

        public DeliveryService(IUnitOfWork unitOfWork, IDeliveryGenerator deliveryGenerator)
        {
            _unitOfWork = unitOfWork;
            _deliveryGenerator = deliveryGenerator;
        }

        #region GETs

        public async Task<Result<List<DeliveryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var deliveries = await _unitOfWork.DeliveryRepository.GetAllAsync(cancellationToken);
            var dtos = deliveries.Select(MapToDto).ToList();
            return Result<List<DeliveryDto>>.Success(dtos);
        }

        public async Task<Result<DeliveryDto>> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            var delivery = await _unitOfWork.DeliveryRepository.GetWithLinesAsync(id, cancellationToken);
            if (delivery == null)
                return Result<DeliveryDto>.Failure($"Delivery Id: {id} is not existed !");
            var dto = MapToDto(delivery);
            return Result<DeliveryDto>.Success(dto);
        }

        public async Task<Result<List<Delivery>>> GetPostedDeliveriesWithLinesAsync(CancellationToken cancellationToken = default)
        {
            var postedDeliveries = await _unitOfWork.DeliveryRepository.GetPostedDeliveriesWithLinesAsync(cancellationToken);
            return Result<List<Delivery>>.Success(postedDeliveries);
        }

        #endregion

        #region CRUDs

        public async Task<Result<DeliveryDto>> CreateAsync(CreateDeliveryDto createDeliveryDto, CancellationToken cancellationToken = default)
        {
            #region Validations

            var existedSalesOrder = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(createDeliveryDto.SalesOrderId, cancellationToken);
            if (existedSalesOrder == null)
                return Result<DeliveryDto>.Failure($"SalesOrder ID: {createDeliveryDto.SalesOrderId} is not existed !");

            if (existedSalesOrder.Status != SalesOrderStatus.Confirmed)
                return Result<DeliveryDto>.Failure($"Only Confirmed Status can be created Delivery - SalesOrder ID: {createDeliveryDto.SalesOrderId} !");

            #endregion

            var salesOrderLines = existedSalesOrder.Lines;
            var deliveriesLines = new List<DeliveryLine>();

            foreach (var deliveryLineDto in createDeliveryDto.LinesDto)
            {
                var salesOrderLine = salesOrderLines
                    .FirstOrDefault(sl => sl.ProductId == deliveryLineDto.ProductId && sl.SalesOrderId == createDeliveryDto.SalesOrderId && deliveryLineDto.RowNumber == sl.RowNumber);

                if (salesOrderLine == null)
                    return Result<DeliveryDto>.Failure($"Product ID {deliveryLineDto.ProductId} is not Exist in SalesOrder {createDeliveryDto.SalesOrderId} !");

                if (deliveryLineDto.DeliveredQty > salesOrderLine.RemainingQty)
                    return Result<DeliveryDto>.Failure("Delivery Qty cannot be greater than RemainingQty");

                deliveriesLines.Add(new DeliveryLine
                {
                    ProductId = deliveryLineDto.ProductId,
                    RowNumber = deliveryLineDto.RowNumber,
                    DeliveredQty = deliveryLineDto.DeliveredQty,
                });
            }

            string orderNumber = await _deliveryGenerator.GenerateAsync(cancellationToken);
            var delivery = new Delivery
            {
                OrderNumber = orderNumber,
                SalesOrderId = createDeliveryDto.SalesOrderId,
                Lines = deliveriesLines
            };

            await _unitOfWork.DeliveryRepository.AddAsync(delivery);
            await _unitOfWork.SaveChangesAsync();

            var dto = MapToDto(delivery);
            return Result<DeliveryDto>.Success(dto);
        }

        public async Task<Result<DeliveryDto>> UpdateAsync(int id, UpdateDeliveryDto updateDeliveryDto, CancellationToken cancellationToken = default)
        {
            #region Validations

            var existedDelivery = await _unitOfWork.DeliveryRepository.GetWithLinesAsync(id, cancellationToken);
            if(existedDelivery == null)
                return Result<DeliveryDto>.Failure($"Delivery ID: {id} is not existed !");

            var existedSalesOrder = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(existedDelivery.SalesOrderId, cancellationToken);
            if (existedSalesOrder == null)
                return Result<DeliveryDto>.Failure($"SalesOrder ID: {existedDelivery.SalesOrderId} is not existed !");

            if (existedDelivery.Status != DeliveryStatus.Draft)
                return Result<DeliveryDto>.Failure($"Only Draft Status can be Update!");

            #endregion

            // Concurrency check: attach client RowVersion so EF can detect conflicts
            if (updateDeliveryDto.RowVersion != null && existedDelivery.RowVersion != null)
            {
                existedDelivery.RowVersion = updateDeliveryDto.RowVersion;
            }

            #region Remove Line Exist is not match New Lines

            var removedDeliveryLines = existedDelivery.Lines
                .Where(e => !updateDeliveryDto.LinesDto.Any(u => u.ProductId == e.ProductId && u.RowNumber == e.RowNumber))
                .ToList();

            foreach (var removedDeliveryLine in removedDeliveryLines)
                existedDelivery.Lines.Remove(removedDeliveryLine);

            #endregion

            foreach (var updateDeliveryLineDto in updateDeliveryDto.LinesDto)
            {
                var salesOrderLine = existedSalesOrder.Lines
                    .FirstOrDefault(sl => sl.ProductId == updateDeliveryLineDto.ProductId && sl.SalesOrderId == existedDelivery.SalesOrderId && sl.RowNumber == updateDeliveryLineDto.RowNumber);
                
                if (salesOrderLine == null)
                    return Result<DeliveryDto>.Failure($"Product ID {updateDeliveryLineDto.ProductId} is not Exist in SalesOrder {salesOrderLine.SalesOrderId} !");

                if (updateDeliveryLineDto.DeliveredQty > salesOrderLine.RemainingQty)
                    return Result<DeliveryDto>.Failure("Delivery Qty cannot be greater than RemainingQty");

                var existedDeliveryLine = existedDelivery.Lines
                    .FirstOrDefault(e => e.ProductId == updateDeliveryLineDto.ProductId && e.RowNumber == updateDeliveryLineDto.RowNumber);
                if(existedDeliveryLine != null)
                {
                    existedDeliveryLine.DeliveredQty = updateDeliveryLineDto.DeliveredQty;
                }
                else
                {
                    var newDeliveryLine = new DeliveryLine
                    {
                        ProductId = updateDeliveryLineDto.ProductId,
                        RowNumber = updateDeliveryLineDto.RowNumber,
                        DeliveredQty = updateDeliveryLineDto.DeliveredQty,
                    };
                    existedDelivery.Lines.Add(newDeliveryLine);
                }
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<DeliveryDto>.Failure("Delivery đã được cập nhật bởi người dùng khác. Vui lòng tải lại dữ liệu và thử lại.");
            }

            var dto = MapToDto(existedDelivery);
            return Result<DeliveryDto>.Success(dto);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var exist = await _unitOfWork.DeliveryRepository.GetByIdAsync(id, cancellationToken);
            if (exist == null)
                return Result.Failure($"Delivery Id: {id} is not existed !");

            exist.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default)
        {
            var isExist = await _unitOfWork.DeliveryRepository.ExistsAsync(x => x.Id == id);
            return Result<bool>.Success(isExist);
        }

        #endregion

        #region Actions

        public async Task<Result> PostAsync(int id, CancellationToken cancellationToken = default)
        {
            const int maxAttempts = 3;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    var delivery = await _unitOfWork.DeliveryRepository.GetWithLinesAsync(id, cancellationToken);
                    if (delivery == null || delivery.Status == DeliveryStatus.Cancelled || delivery.Status == DeliveryStatus.Posted)
                        return Result.Failure("Delivery invalid status!");

                    var salesOrder = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(delivery.SalesOrderId, cancellationToken);
                    if (salesOrder == null || salesOrder.Status == SalesOrderStatus.Completed || salesOrder.Status == SalesOrderStatus.Cancelled)
                        return Result.Failure("Something went wrong with own SalesOrder!");

                    var reservations = await _unitOfWork.InventoryReservationRepository
                        .GetReservationBySource(delivery.SalesOrderId, "SalesOrder", cancellationToken);

                    var costLayerIds = reservations.Select(x => x.LayerId).Distinct().ToList();
                    var costLayers = await _unitOfWork.InventoryCostLayerRepository
                        .GetByIdsAsync(costLayerIds, cancellationToken);

                    // Convert to dictionary
                    var salesOrderLinesDict = salesOrder.Lines
                        .ToDictionary(x => (x.ProductId, x.RowNumber));

                    var reservationDict = reservations
                        .ToDictionary(x => (x.ProductId, x.RowNumber));

                    var costLayerDict = costLayers
                        .ToDictionary(x => x.Id);

                    int completedLines = 0;
                    var journalEntryLines = new List<JournalEntryLine>();

                    foreach (var deliveryLine in delivery.Lines)
                    {
                        var key = (deliveryLine.ProductId, deliveryLine.RowNumber);

                        if (!salesOrderLinesDict.TryGetValue(key, out var salesOrderLine))
                            return Result.Failure($"SalesOrderLine not found Product_{deliveryLine.ProductId} Row_{deliveryLine.RowNumber}");

                        if (deliveryLine.DeliveredQty > salesOrderLine.RemainingQty)
                            return Result.Failure($"Delivery qty greater than SalesOrder remaining qty");

                        // Update SO line
                        salesOrderLine.DeliveredQty += deliveryLine.DeliveredQty;

                        if (salesOrderLine.DeliveredQty >= salesOrderLine.OrderedQty)
                            completedLines++;

                        if (!reservationDict.TryGetValue(key, out var reservation))
                            return Result.Failure($"Reservation not found Product_{deliveryLine.ProductId} Row_{deliveryLine.RowNumber}");

                        if (reservation.ReservedQty < deliveryLine.DeliveredQty)
                            return Result.Failure("Reservation not enough");

                        reservation.ReservedQty -= deliveryLine.DeliveredQty;

                        if (reservation.ReservedQty == 0)
                            reservation.IsDeleted = true;

                        if (!costLayerDict.TryGetValue(reservation.LayerId, out var costLayer))
                            throw new Exception($"Layer {reservation.LayerId} not found");

                        if (costLayer.RemainingQty < deliveryLine.DeliveredQty)
                            return Result.Failure($"Layer {reservation.LayerId} not enough stock");

                        // Reduce layer
                        costLayer.RemainingQty -= deliveryLine.DeliveredQty;
                        costLayer.ReservedQty -= deliveryLine.DeliveredQty;

                        // Update delivery line
                        deliveryLine.UnitCost = costLayer.UnitCost;
                        deliveryLine.CostLayerId = reservation.LayerId;

                        // Ledger
                        var ledger = new InventoryLedger
                        {
                            ProductId = deliveryLine.ProductId,
                            WarehouseId = costLayer.WarehouseId,
                            TransactionType = InventoryTransactionType.Issue,
                            ReferenceId = delivery.Id,
                            ReferenceType = "Delivery",
                            QuantityIn = 0,
                            QuantityOut = deliveryLine.DeliveredQty,
                            UnitCost = costLayer.UnitCost,
                            TotalCost = deliveryLine.LineTotal
                        };

                        await _unitOfWork.InventoryLedgerRepository.AddAsync(ledger);

                        // Accounting
                        var journalCOGS = new JournalEntryLine
                        {
                            AccountId = (int)AccountCode.COGS,
                            Debit = deliveryLine.LineTotal,
                            Credit = 0,
                            Description = $"DEL:{id}: Product {deliveryLine.ProductId} * {deliveryLine.DeliveredQty}"
                        };

                        var journalInventory = new JournalEntryLine
                        {
                            AccountId = (int)AccountCode.Inventory,
                            Debit = 0,
                            Credit = deliveryLine.LineTotal,
                            Description = $"DEL:{id}: Product {deliveryLine.ProductId} * {deliveryLine.DeliveredQty}"
                        };

                        journalEntryLines.Add(journalCOGS);
                        journalEntryLines.Add(journalInventory);
                    }

                    // Update SO status
                    if (completedLines == salesOrder.Lines.Count)
                        salesOrder.Status = SalesOrderStatus.Completed;
                    else
                        salesOrder.Status = SalesOrderStatus.PartiallyDelivered;

                    delivery.Status = DeliveryStatus.Posted;

                    var journalEntry = new JournalEntry
                    {
                        Reference = delivery.OrderNumber,
                        DeliveryId = delivery.Id,
                        Lines = journalEntryLines
                    };

                    await _unitOfWork.JournalEntryRepository.AddAsync(journalEntry);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    return Result.Success();
                }
                catch (DbUpdateConcurrencyException)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                    if (attempt == maxAttempts)
                        return Result.Failure("Inventory / Delivery has changed. Try again!.");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure(ex.Message);
                }
            }

            return Result.Failure("Inventory / Delivery is updated by other users !");
        }

        public async Task<Result> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            var exist = await _unitOfWork.DeliveryRepository.GetByIdAsync(id, cancellationToken);
            if(exist == null)
                return Result.Failure($"Delivery Id: {id} is not existed !");

            exist.Status = DeliveryStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync (cancellationToken);
            return Result.Success();
        }

        #endregion

        #region Helpers

        private static DeliveryDto MapToDto(Delivery entity)
        {
            return new DeliveryDto
            {
                Id = entity.Id,
                OrderNumber = entity.OrderNumber,
                SalesOrderId = entity.SalesOrderId,
                Status = entity.Status,
                DeliveryDate = entity.DeliveryDate,
                TotalAmount = entity.TotalAmount,
                RowVersion = entity.RowVersion,
                LinesDto = entity.Lines
                    .Select(l => new DeliveryLineDto
                    {
                        DeliveryId = entity.Id,
                        ProductId = l.ProductId,
                        DeliveredQty = l.DeliveredQty,
                        InvoicedQty = l.InvoicedQty,
                        UnitCost = l.UnitCost,     
                        LineTotal = l.LineTotal,
                    })
                    .ToList()
            };
        }

        #endregion
    }
}



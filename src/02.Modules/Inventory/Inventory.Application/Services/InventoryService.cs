using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Inventory;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Inventory.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ReleaseReserveQtyInLayers(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var reservations = await _unitOfWork.InventoryReservationRepository.GetReservationBySource(orderId, "Order", cancellationToken);
                var layerIds = reservations.Select(r => r.LayerId).ToList();
                var layers = await _unitOfWork.InventoryCostLayerRepository.GetByIdsAsync(layerIds, cancellationToken);
                var layersDic = layers.ToDictionary(l => l.Id, l => l);

                foreach (var reservation in reservations)
                {
                    int layerId = reservation.LayerId;
                    var reserveQty = reservation.ReservedQty;
                    var layer = layersDic[layerId];

                    // release reservedQty in Layer
                    layer.ReservedQty -= reserveQty;

                    // release InventoryReservation (stock = 0 and finish it : isDelete = 1)
                    reservation.ReservedQty -= reserveQty; // 0
                    reservation.IsDeleted = true;
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                throw;
            }
        }

        public async Task ExportStockInLayers(int orderId, CancellationToken cancellationToken = default)
        {
            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var reservations = await _unitOfWork.InventoryReservationRepository.GetReservationBySource(orderId, "Order", cancellationToken);
                    var layerIds = reservations.Select(r => r.LayerId).ToList();
                    var layers = await _unitOfWork.InventoryCostLayerRepository.GetByIdsAsync(layerIds, cancellationToken);
                    var layersDic = layers.ToDictionary(l => l.Id, l => l);

                    foreach (var reservation in reservations)
                    {
                        int layerId = reservation.LayerId;
                        var reserveQty = reservation.ReservedQty;
                        var layer = layersDic[layerId];

                        layer.RemainingQty -= reserveQty;
                        layer.ReservedQty -= reserveQty;

                        // Update Reservation
                        reservation.ReservedQty -= reserveQty;
                        reservation.IsDeleted = true;

                        // Ledger
                        var ledger = new InventoryLedger
                        {
                            ProductId = reservation.ProductId,
                            WarehouseId = layer.WarehouseId,
                            TransactionType = InventoryTransactionType.Issue,
                            ReferenceId = orderId,
                            ReferenceType = "Order",
                            QuantityIn = 0,
                            QuantityOut = reserveQty,
                            UnitCost = layer.UnitCost,
                            TotalCost = layer.UnitCost * reserveQty
                        };
                        await _unitOfWork.InventoryLedgerRepository.AddAsync(ledger);
                    }

                    // Concurrency handling: If another transaction has modified the same inventory layers, a DbUpdateConcurrencyException will be thrown, and we can retry the operation.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (attempt == maxAttempts) throw;

                    Console.WriteLine($"Concurrency conflict on attempt {attempt}. Retrying...");

                    // RELOAD: get the newest values from database and retry
                    foreach (var entry in ex.Entries)
                    {
                        await entry.ReloadAsync(cancellationToken);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public async Task<List<ReserveDto>> ReserveFIFOAsync(List<FIFOItemDto> items, CancellationToken cancellationToken = default)
        {
            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var reserveDtos = new List<ReserveDto>();
                    int rowNumber = 1;
                    foreach (var item in items)
                    {
                        var layers = await _unitOfWork.InventoryCostLayerRepository.GetAvailableLayersByProductId(item.ProductId, cancellationToken);
                        if (layers == null || layers.Count == 0)
                            throw new Exception($"Product {item.ProductId}: {item.ProductName} not in inventory");

                        decimal remainingOrderedQty = item.NeccessaryQty;

                        foreach (var layer in layers)
                        {
                            if (remainingOrderedQty <= 0)
                                break;

                            var available = layer.RemainingQty - layer.ReservedQty;
                            if (available <= 0) continue;

                            var takeQty = Math.Min(available, remainingOrderedQty);

                            // Update the layer's reserved quantity
                            layer.ReservedQty += takeQty;

                            var reservation = new InventoryReservation
                            {
                                LayerId = layer.Id,
                                ProductId = item.ProductId,
                                SourceId = item.SourceId,
                                SourceType = item.SourceType,
                                RowNumber = rowNumber,
                                ReservedQty = takeQty,
                                UnitCost = layer.UnitCost
                            };
                            await _unitOfWork.InventoryReservationRepository.AddAsync(reservation, cancellationToken);

                            // Adding ReserveDto to the list
                            reserveDtos.Add(new ReserveDto
                            {
                                ProductId = reservation.ProductId,
                                ProductName = item.ProductName,
                                SourceId = reservation.SourceId,
                                ReservedQty = reservation.ReservedQty,
                                UnitCost = reservation.UnitCost,
                                RowNumber = reservation.RowNumber
                            });

                            remainingOrderedQty -= takeQty;
                            rowNumber++;
                        }

                        if (remainingOrderedQty > 0)
                            throw new Exception($"Not enough stock for product {item.ProductId}");
                    }

                    // Concurrency handling: If another transaction has modified the same inventory layers, a DbUpdateConcurrencyException will be thrown, and we can retry the operation.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return reserveDtos;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if(attempt == maxAttempts) throw;

                    // RELOAD: get the newest values from database and retry
                    foreach (var entry in ex.Entries)
                    {
                        await entry.ReloadAsync(cancellationToken);
                    }
                }  
                catch
                {
                    throw;
                }
            }

            return new List<ReserveDto>();
        }
    }
}
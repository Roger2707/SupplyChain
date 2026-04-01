using Inventory.Application.DTOs.SalesOrder;
using Inventory.Application.Interfaces.Generators;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.SalesOrder;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SharedKernel.Entities;
using System.Transactions;

namespace Inventory.Application.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISalesOrderGenerator _salesOrderGenerator;
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;

        public SalesOrderService(IUnitOfWork unitOfWork, ISalesOrderGenerator salesOrderGenerator, IInventoryService inventoryService, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _salesOrderGenerator = salesOrderGenerator;
            _inventoryService = inventoryService;
            _productService = productService;
        }

        #region GET

        public async Task<Result<List<SalesOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var salesOrders = await _unitOfWork.SalesOrderRepository.GetAllWithLinesAsync(cancellationToken);
            var dtos = salesOrders.Select(MapToDto).ToList();
            return Result<List<SalesOrderDto>>.Success(dtos);
        }

        public async Task<Result<SalesOrderDto>> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            var salesOrder = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(id, cancellationToken);
            if (salesOrder == null)
                return Result<SalesOrderDto>.Failure($"SalesOrder with ID: {id} is not existed !");

            var dto = MapToDto(salesOrder);
            return Result<SalesOrderDto>.Success(dto);
        }

        public async Task<Result<List<SalesOrder>>> GetConfirmedSalesOrders(CancellationToken cancellationToken = default)
        {
            var salesOrdersConfirmed = await _unitOfWork.SalesOrderRepository.GetConfirmedSalesOrders(cancellationToken);
            return Result<List<SalesOrder>>.Success(salesOrdersConfirmed);
        }

        #endregion

        #region CRUDs

        public async Task<Result<SalesOrderDto>> CreateAsync(CreateSalesOrderDto createSalesOrderDto, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                #region Validations

                var isCustomerExisted = await _unitOfWork.CustomerRepository
                    .ExistsAsync(c => c.Id == createSalesOrderDto.CustomerId, cancellationToken);

                if (!isCustomerExisted)
                    return Result<SalesOrderDto>.Failure($"Customer ID {createSalesOrderDto.CustomerId} is not Existed !");

                #endregion

                var productIds = createSalesOrderDto.CreateLinesDto.Select(l => l.ProductId).ToList();
                var productsSellingPriceDic = await _productService.GetProductsSellingPrice(productIds, cancellationToken);

                var salesOrderLines = new List<SalesOrderLine>();
                int rowNumber = 1;
                foreach(var create_line in createSalesOrderDto.CreateLinesDto)
                {
                    if(!productsSellingPriceDic.TryGetValue(create_line.ProductId, out var productSellingPrice))
                    {
                        if (productSellingPrice == null)
                            return Result<SalesOrderDto>.Failure($"Product ID {create_line.ProductId} is not Existed !");
                    }
                                        
                    salesOrderLines.Add(
                        new SalesOrderLine
                        {
                            ProductId = create_line.ProductId,                            
                            RowNumber = rowNumber,
                            UnitPrice = productSellingPrice.SellingPrice,
                            OrderedQty = create_line.OrderedQty,
                        }
                    );

                    rowNumber++;
                }

                string orderNumber = await _salesOrderGenerator.GenerateAsync(cancellationToken);
                var salesOrder = new SalesOrder
                {
                    OrderNumber = orderNumber,
                    CustomerId = createSalesOrderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    Lines = salesOrderLines
                };

                await _unitOfWork.SalesOrderRepository.AddAsync(salesOrder);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var dto = MapToDto(salesOrder);
                return Result<SalesOrderDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<SalesOrderDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<SalesOrderDto>> UpdateAsync(int id, UpdateSalesOrderDto updateSalesOrderDto, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                #region Validations

                var salesOrderExist = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(id, cancellationToken);
                if (salesOrderExist == null)
                    return Result<SalesOrderDto>.Failure($"SalesOrder with ID: {id} is not existed !");

                var isCustomerExisted = await _unitOfWork.CustomerRepository.ExistsAsync(c => c.Id == updateSalesOrderDto.CustomerId, cancellationToken);
                if (!isCustomerExisted)
                    return Result<SalesOrderDto>.Failure($"Customer ID {updateSalesOrderDto.CustomerId} is not Existed !");

                bool isAllow = salesOrderExist.AllowUpdate();
                if (!isAllow)
                    return Result<SalesOrderDto>.Failure($"Only Draft Status can be Updated !");

                #endregion

                var productIds = updateSalesOrderDto.UpdateLinesDto.Select(l => l.ProductId).ToList();
                var productsSellingPriceDic = await _productService.GetProductsSellingPrice(productIds, cancellationToken);

                // Concurrency check: attach client RowVersion so EF can detect conflicts
                if (updateSalesOrderDto.RowVersion != null && salesOrderExist.RowVersion != null)
                {
                    // This assignment tells EF which version the client thinks it is editing
                    salesOrderExist.RowVersion = updateSalesOrderDto.RowVersion;
                }

                salesOrderExist.CustomerId = updateSalesOrderDto.CustomerId;
                salesOrderExist.Lines.Clear();

                int rowNumber = 1;
                foreach (var update_line in updateSalesOrderDto.UpdateLinesDto)
                {
                    if (!productsSellingPriceDic.TryGetValue(update_line.ProductId, out var productSellingPrice))
                    {
                        if (productSellingPrice == null)
                            return Result<SalesOrderDto>.Failure($"Product ID {update_line.ProductId} is not Existed !");
                    }

                    salesOrderExist.Lines.Add(new SalesOrderLine
                    {
                        ProductId = update_line.ProductId,
                        RowNumber = rowNumber,
                        UnitPrice = productSellingPrice.SellingPrice,
                        OrderedQty = update_line.OrderedQty,
                    });

                    rowNumber++;
                }

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<SalesOrderDto>.Failure("SalesOrder is updated by other users, please update again !");
                }

                var dto = MapToDto(salesOrderExist);
                return Result<SalesOrderDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<SalesOrderDto>.Failure(ex.Message);
            }
        }        

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var salesOrderExist = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(id, cancellationToken);
            if (salesOrderExist == null)
                return Result.Failure($"SalesOrder with ID: {id} is not existed !");

            salesOrderExist.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default)
        {
            var isExist = await _unitOfWork.SalesOrderRepository.ExistsAsync(s => s.Id == id, cancellationToken);
            return Result<bool>.Success(isExist);
        }

        #endregion

        public async Task<Result> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            var salesOrderExist = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(id, cancellationToken);
            if (salesOrderExist == null)
                return Result.Failure($"SalesOrder with ID: {id} is not existed !");

            salesOrderExist.Status = Domain.Enums.SalesOrderStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> ConfirmAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var so = await _unitOfWork.SalesOrderRepository.GetWithLinesAsync(id, cancellationToken);
                    if (so == null)
                        return Result.Failure($"SalesOrder with ID {id} not found.");

                    // Confirm built in Enitty method, will check status and set to Confirmed if valid, otherwise throw exception
                    so.Confirm();

                    // FIFO Reservation
                    var reserveDtos = await _inventoryService.ReserveFIFOAsync(
                        so.Lines.Select(l => new FIFOItemDto
                        {
                            ProductId = l.ProductId,
                            ProductName = l.Product?.Name,
                            NeccessaryQty = l.OrderedQty,
                            SourceId = l.SalesOrderId,
                            SourceType = "SalesOrder",
                        }).ToList(),
                        cancellationToken);

                    // Map to ProductSellingPrice
                    var productIds = reserveDtos.Select(l => l.ProductId).ToList();
                    var productsSellingPriceDic = await _productService.GetProductsSellingPrice(productIds, cancellationToken);

                    // because reservation may split one line into multiple lines belong to quantity in layers
                    var salesOrderLinesSplit = reserveDtos.Select(r => new SalesOrderLine
                    {
                        SalesOrderId = so.Id,
                        ProductId = r.ProductId,
                        RowNumber = r.RowNumber,
                        UnitCost = r.UnitCost,
                        UnitPrice = productsSellingPriceDic.TryGetValue(r.ProductId, out var productSellingPrice) ? productSellingPrice.SellingPrice : 0,
                        OrderedQty = r.ReservedQty,
                    }).ToList();

                    // Re-check SalesOrder 
                    so.Lines.Clear();
                    so.Lines.AddRange(salesOrderLinesSplit);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    scope.Complete();
                    return Result.Success();
                }
            }
            catch (Exception ex)
            {
                // If there 's any exception
                // TransactionScope will be rolled back automatically, ensuring data consistency.
                return Result.Failure(ex.Message);
            }
        }

        #region Helpers

        private static SalesOrderDto MapToDto(SalesOrder entity)
        {
            return new SalesOrderDto
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                OrderDate = entity.OrderDate,
                Status = entity.Status,
                CustomerName = entity.Customer?.CustomerName,
                TotalAmount = entity.TotalAmount,
                LinesDto = entity.Lines.Select(l => new SalesOrderLineDto
                {
                    ProductId = l.ProductId,
                    ProductName = l.Product?.Name,
                    DeliveredQty = l.DeliveredQty,
                    OrderedQty = l.OrderedQty,
                    RemainingQty = l.RemainingQty,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal,
                }).ToList(),
            };
        }        

        #endregion
    }
}



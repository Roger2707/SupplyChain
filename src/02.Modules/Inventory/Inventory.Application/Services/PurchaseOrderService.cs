using Inventory.Application.DTOs.PurchaseOrders;
using Inventory.Application.Interfaces.Generators;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.PurchaseOrder;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;
using SharedKernel.Ultilities;

namespace Inventory.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPurchaseOrderGenerator _purchaseOrderGenerator;

    public PurchaseOrderService(IUnitOfWork unitOfWork, IPurchaseOrderGenerator purchaseOrderGenerator)
    {
        _unitOfWork = unitOfWork;
        _purchaseOrderGenerator = purchaseOrderGenerator;
    }

    public async Task<Result<IEnumerable<PurchaseOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.PurchaseOrderRepository.GetAllWithLinesAsync(cancellationToken);
        var dtos = entities.Select(MapToDto);
        return Result<IEnumerable<PurchaseOrderDto>>.Success(dtos);
    }

    public async Task<Result<PurchaseOrderDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.PurchaseOrderRepository.GetWithLinesAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result<PurchaseOrderDto>.Failure($"PurchaseOrder with ID {id} not found.");
        }

        var dto = MapToDto(entity);
        return Result<PurchaseOrderDto>.Success(dto);
    }

    public async Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto createPurchaseOrder, CancellationToken cancellationToken = default)
    {
        if(!await _unitOfWork.SupplierRepository.ExistsAsync(s => s.Id == createPurchaseOrder.SupplierId))
            return Result<PurchaseOrderDto>.Failure($"Supplier Id : {createPurchaseOrder.SupplierId} is not existed .");

        foreach (var line in createPurchaseOrder.LinesDto)
        {
            if (!await _unitOfWork.ProductRepository.ExistsAsync(p => p.Id == line.ProductId))
                return Result<PurchaseOrderDto>.Failure($"Product Id : {line.ProductId} is not existed .");
        }

        var lines = new List<PurchaseOrderLine>();
        foreach(var pol in createPurchaseOrder.LinesDto)
        {
            var unitPrice = await _unitOfWork.SupplierProductPriceRepository.GetTopUnitPrice(createPurchaseOrder.SupplierId, pol.ProductId, cancellationToken);

            var line = new PurchaseOrderLine
            {
                ProductId = pol.ProductId,
                OrderedQty = pol.OrderedQty,
                UnitPrice = unitPrice,
                ReceivedQty = 0,
            };

            lines.Add(line);
        }

        var po = new PurchaseOrder
        {
            OrderNumber = await _purchaseOrderGenerator.GenerateAsync(cancellationToken),
            SupplierId = createPurchaseOrder.SupplierId,
            OrderDate = DateTime.UtcNow,
            Status = PurchaseOrderStatus.Draft,
            Lines = lines,
            TotalAmount = lines.Sum(l => l.LineTotal)
        };

        await _unitOfWork.PurchaseOrderRepository.AddAsync(po, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(po);
        return Result<PurchaseOrderDto>.Success(dto);
    }

    public async Task<Result<PurchaseOrderDto>> UpdateAsync(int id, UpdatePurchaseOrderDto updatePurchaseOrder, CancellationToken cancellationToken = default)
    {
        var exist = await _unitOfWork.PurchaseOrderRepository.GetWithLinesAsync(id, cancellationToken);
        if (exist == null)
            return Result<PurchaseOrderDto>.Failure($"PurchaseOrder with ID {id} not found.");

        if (updatePurchaseOrder.RowVersion != null && exist.RowVersion != null)
        {
            exist.RowVersion = updatePurchaseOrder.RowVersion;
        }

        var lineParams = new List<(int ProductId, decimal OrderedQty, decimal UnitPrice)>();
        foreach (var line in updatePurchaseOrder.LinesDto)
        {
            if (!await _unitOfWork.ProductRepository.ExistsAsync(p => p.Id == line.ProductId))
                return Result<PurchaseOrderDto>.Failure($"Product Id : {line.ProductId} is not existed .");

            var unitPrice = await _unitOfWork.SupplierProductPriceRepository.GetTopUnitPrice(updatePurchaseOrder.SupplierId, line.ProductId, cancellationToken);
            
            lineParams.Add((line.ProductId, line.OrderedQty, unitPrice));
        }

        exist.Update(
            updatePurchaseOrder.SupplierId, 
            updatePurchaseOrder.OrderDate,
            lineParams.Select(l => (CF.GetInt(l.ProductId), CF.GetDecimal(l.OrderedQty), CF.GetDecimal(l.UnitPrice))).ToList());

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PurchaseOrderDto>.Failure("PurchaseOrder is updated by other users, please update again !");
        }

        return Result<PurchaseOrderDto>.Success(MapToDto(exist));
    }

    public async Task<Result<PurchaseOrderDto>> ApprovePurchaseOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var po = await _unitOfWork.PurchaseOrderRepository.GetWithLinesAsync(id, cancellationToken);
        if (po == null)
            return Result<PurchaseOrderDto>.Failure($"PurchaseOrder with ID {id} not found.");

        // Ensure the PO is in a state that can be approved
        po.Approve();

        // After approve, we have to create goods_receipt

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PurchaseOrderDto>.Failure("PurchaseOrder is updated by other users, please update again !");
        }

        var dto = MapToDto(po);
        return Result<PurchaseOrderDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var po = await _unitOfWork.PurchaseOrderRepository.GetByIdAsync(id, cancellationToken);
        if (po == null)
            return Result.Failure($"PurchaseOrder with ID {id} not found.");

        if (po.Status != PurchaseOrderStatus.Draft)
            return Result.Failure("Only Draft PurchaseOrder can be deleted.");

        po.Delete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.PurchaseOrderRepository.ExistsAsync(po => po.Id == id, cancellationToken);
        return Result<bool>.Success(exists);
    }

    #region Helpers

    private static PurchaseOrderDto MapToDto(PurchaseOrder entity)
    {
        return new PurchaseOrderDto
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            SupplierId = entity.SupplierId,
            Status = entity.Status,
            OrderDate = entity.OrderDate,
            ApprovedDate = entity.ApprovedDate,
            TotalAmount = entity.TotalAmount,
            RowVersion = entity.RowVersion,
            Lines = entity.Lines?.Select(l => new PurchaseOrderLineDto
            {
                PurchaseOrderId = l.PurchaseOrderId,
                ProductId = l.ProductId,
                OrderedQty = l.OrderedQty,
                ReceivedQty = l.ReceivedQty,
                UnitPrice = l.UnitPrice,
                LineTotal = l.OrderedQty * l.UnitPrice
            }).ToList() ?? new List<PurchaseOrderLineDto>()
        };
    }

    #endregion
}




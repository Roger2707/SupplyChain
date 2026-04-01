using Inventory.Application.DTOs.GoodsReceipts;
using Inventory.Application.Interfaces.Generators;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Accounts;
using Inventory.Domain.Entities.GoodsReceipt;
using Inventory.Domain.Entities.Inventory;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Inventory.Application.Services;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoodsReceiptGenerator _goodsReceiptGenerator;
    private readonly IProductService _productService;

    public GoodsReceiptService(IUnitOfWork unitOfWork, IGoodsReceiptGenerator goodsReceiptGenerator, IProductService productService)
    {
        _unitOfWork = unitOfWork;
        _goodsReceiptGenerator = goodsReceiptGenerator;
        _productService = productService;
    }

    #region Get

    public async Task<Result<IEnumerable<GoodsReceiptDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.GoodsReceiptRepository.GetAllWithLinesAsync(cancellationToken);
        var dtos = entities.Select(MapToDto);
        return Result<IEnumerable<GoodsReceiptDto>>.Success(dtos);
    }

    public async Task<Result<GoodsReceiptDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.GoodsReceiptRepository.GetWithLinesAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result<GoodsReceiptDto>.Failure($"GoodsReceipt with ID {id} not found.");
        }

        var dto = MapToDto(entity);
        return Result<GoodsReceiptDto>.Success(dto);
    }

    #endregion

    #region CRUD

    public async Task<Result<GoodsReceiptDto>> CreateAsync(CreateGoodsReceiptDto createGoodsReceipt, CancellationToken cancellationToken = default)
    {
        var poExist = await _unitOfWork.PurchaseOrderRepository.GetWithLinesAsync(createGoodsReceipt.PurchaseOrderId, cancellationToken);
        if (poExist == null)
            return Result<GoodsReceiptDto>.Failure($"PurchaseOrder with ID {createGoodsReceipt.PurchaseOrderId} not found.");

        var warehouseExist = await _unitOfWork.WarehouseRepository.ExistsAsync(w => w.Id == createGoodsReceipt.WarehouseId, cancellationToken);
        if (!warehouseExist)
            return Result<GoodsReceiptDto>.Failure($"Warehouse with ID {createGoodsReceipt.WarehouseId} not found.");

        foreach(var line in createGoodsReceipt.LinesDto)
        {
            // Check if the PurchaseOrderLine exists and belongs to the specified PurchaseOrder
            var lineExist = poExist.Lines.FirstOrDefault(po => po.PurchaseOrderId == line.PurchaseOrderId && po.ProductId == line.ProductId);
            if (lineExist == null)
                return Result<GoodsReceiptDto>.Failure($"PurchaseOrderLine with ProductID {line.ProductId} not found in PurchaseOrder {createGoodsReceipt.PurchaseOrderId}.");

            // Check if the Product exists
            var productExist = await _unitOfWork.ProductRepository.ExistsAsync(p => p.Id == line.ProductId, cancellationToken);
            if (!productExist)
                return Result<GoodsReceiptDto>.Failure($"Product with ID {line.ProductId} not found.");

            if(line.ReceivedQty <= 0)
                return Result<GoodsReceiptDto>.Failure($"Received quantity must be greater than zero for Product ID {line.ProductId}.");

            if(line.ReceivedQty > lineExist.OrderedQty)
                return Result<GoodsReceiptDto>.Failure($"Received quantity for Product ID {line.ProductId} cannot exceed the ordered quantity.");
        }

        var receiptNumber = await _goodsReceiptGenerator.GenerateAsync(cancellationToken);
        var lines = createGoodsReceipt.LinesDto.Select(l => new GoodsReceiptLine
        {
            PurchaseOrderId = poExist.Id,
            ProductId = l.ProductId,
            ReceivedQty = l.ReceivedQty,
            UnitCost = poExist.Lines.FirstOrDefault(pol => pol.ProductId == l.ProductId).UnitPrice,
        }).ToList();

        var entity = new GoodsReceipt
        {
            ReceiptNumber = receiptNumber,
            PurchaseOrderId = createGoodsReceipt.PurchaseOrderId,
            WarehouseId = createGoodsReceipt.WarehouseId,
            ReceiptDate = DateTime.UtcNow,
            Status = ReceiptStatus.Draft,
            Lines = lines,
            TotalAmount = lines.Sum(l => l.LineTotal)
        };

        await _unitOfWork.GoodsReceiptRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(entity);
        return Result<GoodsReceiptDto>.Success(dto);
    }

    public async Task<Result<GoodsReceiptDto>> UpdateAsync(int id, UpdateGoodsReceiptDto updateGoodsReceipt, CancellationToken cancellationToken = default)
    {
        var exist = await _unitOfWork.GoodsReceiptRepository.GetWithLinesAsync(id, cancellationToken);
        if (exist == null)
            return Result<GoodsReceiptDto>.Failure($"GoodsReceipt with ID {id} not found.");

        if (updateGoodsReceipt.RowVersion != null && exist.RowVersion != null)
        {
            exist.RowVersion = updateGoodsReceipt.RowVersion;
        }

        var poExist = await _unitOfWork.PurchaseOrderRepository.GetWithLinesAsync(exist.PurchaseOrderId, cancellationToken);
        if (poExist == null)
            return Result<GoodsReceiptDto>.Failure($"PurchaseOrder with ID {exist.PurchaseOrderId} not found.");

        var warehouseExist = await _unitOfWork.WarehouseRepository.ExistsAsync(w => w.Id == updateGoodsReceipt.WarehouseId, cancellationToken);
        if (!warehouseExist)
            return Result<GoodsReceiptDto>.Failure($"Warehouse with ID {updateGoodsReceipt.WarehouseId} not found.");

        var lineParams = new List<(int ProductId, decimal orderedQty, decimal ReceivedQty, decimal UnitCost)>();
        foreach (var line in updateGoodsReceipt.LinesDto)
        {
            // Check if the PurchaseOrderLine exists and belongs to the specified PurchaseOrder
            var poLineExist = poExist.Lines.FirstOrDefault(po => po.ProductId == line.ProductId);
            if (poLineExist == null)
                return Result<GoodsReceiptDto>.Failure($"PurchaseOrderLine with ProductID {line.ProductId} not found in PurchaseOrder {poExist.Id}.");

            if (line.ReceivedQty <= 0)
                return Result<GoodsReceiptDto>.Failure($"Received quantity must be greater than zero for Product ID {line.ProductId}.");

            lineParams.Add((line.ProductId, poLineExist.OrderedQty, line.ReceivedQty, poLineExist.UnitPrice));
        }

        exist.Update(
            updateGoodsReceipt.WarehouseId,
            DateTime.Now,
            lineParams
        );

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<GoodsReceiptDto>.Failure("GoodsReceipt is updated by other user. Let's try again.");
        }
        var dto = MapToDto(exist);
        return Result<GoodsReceiptDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var exist = await _unitOfWork.GoodsReceiptRepository.GetWithLinesAsync(id, cancellationToken);
        if (exist == null)
            return Result.Failure($"GoodsReceipt with ID {id} not found.");

        exist.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
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

                var goodsReceiptExist = await _unitOfWork.GoodsReceiptRepository.GetWithLinesAsync(id, cancellationToken);
                if (goodsReceiptExist == null)
                    return Result.Failure($"GoodsReceipt with Id: {id} is not existed");

                var purchaseOrder = await _unitOfWork.PurchaseOrderRepository.GetWithLinesAsync(goodsReceiptExist.PurchaseOrderId, cancellationToken);
                if (purchaseOrder == null)
                    return Result.Failure($"PurchaseOrder with Id: {goodsReceiptExist.PurchaseOrderId} is not existed");

                if (purchaseOrder.Status == PurchaseOrderStatus.Draft)
                    return Result.Failure($"PurchaseOrder is not Approved !");

                if (purchaseOrder.Status == PurchaseOrderStatus.Completed)
                    return Result.Failure($"PurchaseOrder is already completed !");

                if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled)
                    return Result.Failure($"PurchaseOrder is Cancelled !");

                goodsReceiptExist.Post();

                int countLineComplete = 0;
                var journalEntryLines = new List<JournalEntryLine>();

                foreach (var line in goodsReceiptExist.Lines)
                {
                    var purchaseLine = purchaseOrder.Lines.FirstOrDefault(l => l.ProductId == line.ProductId);

                    if (purchaseLine == null)
                        throw new Exception("PurchaseOrderLine not found !");

                    if (purchaseLine.ReceivedQty == purchaseLine.OrderedQty)
                        throw new Exception("This Purchase has complete");

                    decimal pol_OrderedQty = purchaseLine.OrderedQty;
                    decimal pol_ReceivedQty = purchaseLine.ReceivedQty;
                    decimal remaining = pol_OrderedQty - pol_ReceivedQty;

                    if (remaining < line.ReceivedQty)
                        throw new Exception("Exceed !");

                    // Update quantity PurchaseOrderLine
                    purchaseLine.ReceivedQty += line.ReceivedQty;

                    if (purchaseLine.ReceivedQty == pol_OrderedQty)
                        countLineComplete++;

                    // Import into CostLayer
                    var costLayer = new InventoryCostLayer
                    {
                        GoodsReceiptId = goodsReceiptExist.Id,
                        ProductId = line.ProductId,
                        WarehouseId = goodsReceiptExist.WarehouseId,
                        OriginalQty = line.ReceivedQty,
                        RemainingQty = line.ReceivedQty,
                        UnitCost = line.UnitCost,
                        ReceiptDate = goodsReceiptExist.ReceiptDate,
                    };

                    await _unitOfWork.InventoryCostLayerRepository.AddAsync(costLayer, cancellationToken);

                    // Create History Transaction
                    var ledger = new InventoryLedger
                    {
                        ProductId = line.ProductId,
                        WarehouseId = goodsReceiptExist.WarehouseId,
                        TransactionType = InventoryTransactionType.Receipt,
                        ReferenceId = goodsReceiptExist.Id,
                        ReferenceType = "GoodsReceipt",
                        QuantityIn = line.ReceivedQty,
                        QuantityOut = 0,
                        UnitCost = line.UnitCost,
                        TotalCost = line.ReceivedQty * line.UnitCost,
                        TransactionDate = goodsReceiptExist.ReceiptDate
                    };

                    await _unitOfWork.InventoryLedgerRepository.AddAsync(ledger, cancellationToken);

                    // Insert JournalEntry
                    var journalEntryLine = new JournalEntryLine
                    {
                        AccountId = (int)AccountCode.Inventory,
                        Debit = line.LineTotal,
                        Credit = 0,
                        Description = $"GR:{id}: Product: {line.ProductId} * {line.ReceivedQty}"
                    };
                    journalEntryLines.Add(journalEntryLine);
                }

                // Update PurchaseOrder_Status
                if (countLineComplete == purchaseOrder.Lines.Count)
                    purchaseOrder.Status = PurchaseOrderStatus.Completed;
                else
                    purchaseOrder.Status = PurchaseOrderStatus.PartiallyReceived;

                // Set SellingPrice for Product
                foreach (var line in goodsReceiptExist.Lines)
                {
                    int productId = line.ProductId;
                    await _productService.SetProductSellingPrice(productId, cancellationToken);
                }

                // Create JournalEntry
                var creditLine = new JournalEntryLine
                {
                    AccountId = (int)AccountCode.Cash, // Cash
                    Credit = journalEntryLines.Sum(x => x.Debit),
                };
                journalEntryLines.Add(creditLine);
                var journalEntry = new JournalEntry
                {
                    Reference = goodsReceiptExist.ReceiptNumber,
                    GoodsReceiptId = goodsReceiptExist.Id,
                    Lines = journalEntryLines
                };
                await _unitOfWork.JournalEntryRepository.AddAsync(journalEntry, cancellationToken);

                // SaveChange
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                if (attempt == maxAttempts)
                    return Result.Failure("GoodsReceipt/PurchaseOrder by other transaction. Try again !");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(ex.Message);
            }
        }

        return Result.Failure("GoodsReceipt/PurchaseOrder by other transaction. Try again !");
    }

    public Task<Result> CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default)
    {
        var isExist = await _unitOfWork.GoodsReceiptRepository.ExistsAsync(g => g.Id == id);
        return Result<bool>.Success(isExist);
    }

    #endregion

    #region Helpers

    private static GoodsReceiptDto MapToDto(GoodsReceipt entity)
    {
        return new GoodsReceiptDto
        {
            Id = entity.Id,
            ReceiptNumber = entity.ReceiptNumber,
            PurchaseOrderId = entity.PurchaseOrderId,
            WarehouseId = entity.WarehouseId,
            Status = entity.Status,
            ReceiptDate = entity.ReceiptDate,
            RowVersion = entity.RowVersion,
            Lines = entity.Lines?.Select(l => new GoodsReceiptLineDto
            {
                GoodsReceiptId = l.GoodsReceiptId,
                PurchaseOrderLId = l.PurchaseOrderId,
                ProductId = l.ProductId,
                ReceivedQty = l.ReceivedQty,
                UnitCost = l.UnitCost,
                LineTotal = l.ReceivedQty * l.UnitCost
            }).ToList() ?? new List<GoodsReceiptLineDto>()
        };
    }

    #endregion
}




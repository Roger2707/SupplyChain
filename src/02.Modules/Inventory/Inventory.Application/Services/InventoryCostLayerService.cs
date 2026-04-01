using Inventory.Application.DTOs.Inventory;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Inventory;
using SharedKernel.Entities;

namespace Inventory.Application.Services;

public class InventoryCostLayerService : IInventoryCostLayerService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryCostLayerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<InventoryCostLayerDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.InventoryCostLayerRepository.GetAllAsync(cancellationToken);
        var dtos = entities.Select(MapToDto).ToList();
        return Result<IEnumerable<InventoryCostLayerDto>>.Success(dtos);
    }

    public async Task<Result<InventoryCostLayerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryCostLayerRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result<InventoryCostLayerDto>.Failure($"InventoryCostLayer with ID {id} not found.");
        }
        var dto = MapToDto(entity);
        return Result<InventoryCostLayerDto>.Success(dto);
    }

    #region Helpers

    private InventoryCostLayerDto MapToDto(InventoryCostLayer entity)
    {
        return new InventoryCostLayerDto
        {
            GoodsReceiptId = entity.GoodsReceiptId,
            ProductId = entity.ProductId,
            WarehouseId = entity.WarehouseId,
            OriginalQty = entity.OriginalQty,
            RemainingQty = entity.RemainingQty,
            UnitCost = entity.UnitCost,
            ReceiptDate = entity.ReceiptDate,
            IsClosed = entity.IsClosed,
        };
    }

    #endregion
}



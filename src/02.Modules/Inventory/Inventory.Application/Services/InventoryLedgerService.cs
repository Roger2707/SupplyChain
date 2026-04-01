using Inventory.Application.DTOs.Inventory;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Inventory;
using SharedKernel.Entities;

namespace Inventory.Application.Services;

public class InventoryLedgerService : IInventoryLedgerService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryLedgerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<InventoryLedgerDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.InventoryLedgerRepository.GetAllAsync(cancellationToken);
        var dtos = entities.Select(MapToDto).ToList();
        return Result<IEnumerable<InventoryLedgerDto>>.Success(dtos);
    }

    public async Task<Result<InventoryLedgerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryLedgerRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result<InventoryLedgerDto>.Failure($"InventoryLedger with ID {id} not found.");
        }

        var dto = MapToDto(entity);
        return Result<InventoryLedgerDto>.Success(dto);
    }

    #region Helper

    private InventoryLedgerDto MapToDto(InventoryLedger entity)
    {
        return new InventoryLedgerDto
        {
            ProductId = entity.ProductId,
            WarehouseId = entity.WarehouseId,
            TransactionType = entity.TransactionType,
            ReferenceId = entity.ReferenceId,
            ReferenceType = entity.ReferenceType,
            QuantityIn = entity.QuantityIn,
            QuantityOut = entity.QuantityOut,
            UnitCost = entity.UnitCost,
            TotalCost = entity.TotalCost,
            TransactionDate = entity.TransactionDate,
        };
    }

    #endregion
}



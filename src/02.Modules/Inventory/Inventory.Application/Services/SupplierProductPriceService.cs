using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Suppliers;
using SharedKernel.Entities;

namespace Inventory.Application.Services;

public class SupplierProductPriceService : ISupplierProductPriceService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierProductPriceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<SupplierProductPrice>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.SupplierProductPriceRepository.GetAllAsync(cancellationToken);
        return Result<IEnumerable<SupplierProductPrice>>.Success(entities);
    }

    public async Task<Result<SupplierProductPrice>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.SupplierProductPriceRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result<SupplierProductPrice>.Failure($"SupplierProductPrice with ID {id} not found.");
        }

        return Result<SupplierProductPrice>.Success(entity);
    }
}




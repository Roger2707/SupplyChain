using Inventory.Application.DTOs.Warehouses;
using Inventory.Application.Interfaces;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Inventory.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WarehouseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(id, cancellationToken);
        
        if (warehouse == null)
        {
            return Result<WarehouseDto>.Failure($"Warehouse with ID {id} not found.");
        }

        var warehouseDto = MapToDto(warehouse);
        return Result<WarehouseDto>.Success(warehouseDto);
    }

    public async Task<Result<IEnumerable<WarehouseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await _unitOfWork.WarehouseRepository.GetAllAsync(cancellationToken);
        var warehouseDtos = warehouses.Select(MapToDto);
        return Result<IEnumerable<WarehouseDto>>.Success(warehouseDtos);
    }

    public async Task<Result<WarehouseDto>> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default)
    {
        var warehouseCodes = await _unitOfWork.WarehouseRepository.GetAllWarehouseCodeAsync(cancellationToken);
        string generatedWarehouseCode = GenerateWarehouseCode(warehouseCodes);

        // Code Empty Check
        if (string.IsNullOrWhiteSpace(generatedWarehouseCode))
            return Result<WarehouseDto>.Failure("Failed to generate a valid warehouse code.");

        // Code Exists Check
        if (await _unitOfWork.WarehouseRepository.ExistsByCodeAsync(generatedWarehouseCode, cancellationToken))
            return Result<WarehouseDto>.Failure($"Warehouse with code '{generatedWarehouseCode}' already exists.");

        var warehouse = MapToEntity(createDto, generatedWarehouseCode);
        await _unitOfWork.WarehouseRepository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var warehouseDto = MapToDto(warehouse);
        return Result<WarehouseDto>.Success(warehouseDto);
    }

    public async Task<Result<WarehouseDto>> UpdateAsync(int id, UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default)
    {
        var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(id, cancellationToken);
        
        if (warehouse == null)
        {
            return Result<WarehouseDto>.Failure($"Warehouse with ID {id} not found.");
        }

        if (updateDto.RowVersion != null && warehouse.RowVersion != null)
        {
            warehouse.RowVersion = updateDto.RowVersion;
        }

        // Check if warehouse code is being changed and if new code already exists
        if (warehouse.WarehouseCode != updateDto.WarehouseCode)
        {
            var existingWarehouse = await _unitOfWork.WarehouseRepository.GetByCodeAsync(updateDto.WarehouseCode, cancellationToken);
            if (existingWarehouse != null && existingWarehouse.Id != id)
            {
                return Result<WarehouseDto>.Failure($"Warehouse with code '{updateDto.WarehouseCode}' already exists.");
            }
        }

        MapToEntity(updateDto, warehouse);
        await _unitOfWork.WarehouseRepository.UpdateAsync(warehouse, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<WarehouseDto>.Failure("Warehouse đã được cập nhật bởi người dùng khác. Vui lòng tải lại dữ liệu và thử lại.");
        }

        var warehouseDto = MapToDto(warehouse);
        return Result<WarehouseDto>.Success(warehouseDto);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(id, cancellationToken);
        
        if (warehouse == null)
        {
            return Result.Failure($"Warehouse with ID {id} not found.");
        }

        // Soft delete
        warehouse.IsDeleted = true;
        await _unitOfWork.WarehouseRepository.UpdateAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.WarehouseRepository.ExistsAsync(w => w.Id == id, cancellationToken);
        return Result<bool>.Success(exists);
    }

    // Manual mapping methods
    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            Id = warehouse.Id,
            WarehouseCode = warehouse.WarehouseCode,
            WarehouseName = warehouse.WarehouseName,
            Address = warehouse.Address,
            PhoneNumber = warehouse.PhoneNumber,
            IsActive = warehouse.IsActive,
            Description = warehouse.Description,
            ManagerId = warehouse.ManagerId,
            CreatedAt = warehouse.CreatedAt,
            UpdatedAt = warehouse.UpdatedAt,
            RowVersion = warehouse.RowVersion
        };
    }

    private static Warehouse MapToEntity(CreateWarehouseDto createDto, string warehouseCode)
    {
        return new Warehouse
        {
            WarehouseCode = warehouseCode,
            WarehouseName = createDto.WarehouseName,
            Address = createDto.Address,
            PhoneNumber = createDto.PhoneNumber,
            IsActive = createDto.IsActive,
            Description = createDto.Description
        };
    }

    private static void MapToEntity(UpdateWarehouseDto updateDto, Warehouse warehouse)
    {
        warehouse.WarehouseCode = updateDto.WarehouseCode;
        warehouse.WarehouseName = updateDto.WarehouseName;
        warehouse.Address = updateDto.Address;
        warehouse.PhoneNumber = updateDto.PhoneNumber;
        warehouse.IsActive = updateDto.IsActive;
        warehouse.Description = updateDto.Description;
    }

    private string GenerateWarehouseCode(List<string> codes)
    {
        if (codes == null || !codes.Any())
            return "WH - 001";

        int maxNumber = codes
            .Select(x => x.Split('-')[1].Trim())
            .Select(x => int.Parse(x))            
            .Max();                               

        int nextNumber = maxNumber + 1;
        return $"WH - {nextNumber:D3}";
    }
}




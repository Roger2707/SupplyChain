using Inventory.Application.DTOs.Suppliers;
using Inventory.Application.Interfaces;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Inventory.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplierService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.SupplierRepository.ExistsAsync(w => w.Id == id, cancellationToken);
            return Result<bool>.Success(exists);
        }
        public async Task<Result<IEnumerable<SupplierDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var suppliers = await _unitOfWork.SupplierRepository.GetAllAsync(cancellationToken);
            var supplierDtos = suppliers.Select(MapToDto);
            return Result<IEnumerable<SupplierDto>>.Success(supplierDtos);
        }

        public async Task<Result<SupplierDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _unitOfWork.SupplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
            {
                return Result<SupplierDto>.Failure($"Supplier with ID {id} not found.");
            }
            var supplierDto = MapToDto(supplier);

            return Result<SupplierDto>.Success(supplierDto);
        }

        public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierDto createDto, CancellationToken cancellationToken = default)
        {
            var codes = await _unitOfWork.SupplierRepository.GetAllSupplierCodeAsync(cancellationToken);
            string generatedCode = GenerateSupplierCode(codes);

            // Code Empty Check
            if (string.IsNullOrWhiteSpace(generatedCode))
                return Result<SupplierDto>.Failure("Failed to generate a valid Supplier code.");

            // Code Exists Check
            if (await _unitOfWork.SupplierRepository.ExistsByCodeAsync(generatedCode, cancellationToken))
                return Result<SupplierDto>.Failure($"Supplier with code '{generatedCode}' already exists.");

            var supplier = MapToEntity(createDto, generatedCode);
            await _unitOfWork.SupplierRepository.AddAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var SupplierDto = MapToDto(supplier);
            return Result<SupplierDto>.Success(SupplierDto);
        }

        public async Task<Result<SupplierDto>> UpdateAsync(int id, UpdateSupplierDto updateDto, CancellationToken cancellationToken = default)
        {
            var supplier = await _unitOfWork.SupplierRepository.GetByIdAsync(id, cancellationToken);

            if (supplier == null)
            {
                return Result<SupplierDto>.Failure($"Supplier with ID {id} not found.");
            }

            if (updateDto.RowVersion != null && supplier.RowVersion != null)
            {
                supplier.RowVersion = updateDto.RowVersion;
            }

            // Check if Supplier code is being changed and if new code already exists
            if (supplier.SupplierCode != updateDto.SupplierCode)
            {
                var existingWarehouse = await _unitOfWork.WarehouseRepository.GetByCodeAsync(updateDto.SupplierCode, cancellationToken);
                if (existingWarehouse != null && existingWarehouse.Id != id)
                {
                    return Result<SupplierDto>.Failure($"Supplier with code '{updateDto.SupplierCode}' already exists.");
                }
            }

            MapToEntity(updateDto, supplier);
            await _unitOfWork.SupplierRepository.UpdateAsync(supplier, cancellationToken);
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<SupplierDto>.Failure("Supplier đã được cập nhật bởi người dùng khác. Vui lòng tải lại dữ liệu và thử lại.");
            }

            var supplierDto = MapToDto(supplier);
            return Result<SupplierDto>.Success(supplierDto);
        }
        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _unitOfWork.SupplierRepository.GetByIdAsync(id, cancellationToken);

            if (supplier == null)
            {
                return Result.Failure($"Supplier with ID {id} not found.");
            }

            // Soft delete
            supplier.IsDeleted = true;
            await _unitOfWork.SupplierRepository.UpdateAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        #region Helpers

        private static Supplier MapToEntity(CreateSupplierDto createDto, string supplierCode)
        {
            return new Supplier
            {
                SupplierCode = supplierCode,
                SupplierName = createDto.SupplierName,
                Address = createDto.Address,
                PhoneNumber = createDto.PhoneNumber,
                IsActive = createDto.IsActive,
                Description = createDto.Description
            };
        }

        private static void MapToEntity(UpdateSupplierDto updateDto, Supplier supplier)
        {
            supplier.SupplierCode = updateDto.SupplierCode;
            supplier.SupplierName = updateDto.SupplierName;
            supplier.Address = updateDto.Address;
            supplier.PhoneNumber = updateDto.PhoneNumber;
            supplier.IsActive = updateDto.IsActive;
            supplier.Description = updateDto.Description;
        }

        private static SupplierDto MapToDto(Supplier supplier)
        {
            return new SupplierDto
            {
                Id = supplier.Id,
                SupplierCode = supplier.SupplierCode,
                SupplierName = supplier.SupplierName,
                Address = supplier.Address,
                PhoneNumber = supplier.PhoneNumber,
                IsActive = supplier.IsActive,
                Description = supplier.Description,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt,
                RowVersion = supplier.RowVersion
            };
        }

        private string GenerateSupplierCode(List<string> codes)
        {
            if (codes == null || !codes.Any())
                return "CUS - 001";

            int maxNumber = codes
                .Select(x => x.Split('-')[1].Trim())
                .Select(x => int.Parse(x))
                .Max();

            int nextNumber = maxNumber + 1;
            return $"CUS - {nextNumber:D3}";
        }

        #endregion
    }
}



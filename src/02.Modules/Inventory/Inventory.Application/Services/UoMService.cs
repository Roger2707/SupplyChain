using Inventory.Application.DTOs.UoMs;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Products;
using SharedKernel.Entities;

namespace Inventory.Application.Services
{
    public class UoMService : IUoMService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UoMService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<UoMDto>>> GetAllUoMsAsync(CancellationToken cancellationToken)
        {
            var uOMs = await _unitOfWork.UoMRepository.GetAllAsync(cancellationToken);
            var dtos = uOMs.Select(u => new UoMDto
            {
                Id = u.Id,
                Name = u.Name
            }).ToList();

            return Result<List<UoMDto>>.Success(dtos);
        }

        public async Task<Result<UoMDto>> GetUoMByIdAsync(int id, CancellationToken cancellationToken)
        {
            var uOM = await _unitOfWork.UoMRepository.GetByIdAsync(id, cancellationToken);
            if (uOM == null)
                return Result<UoMDto>.Failure($"UOM with ID {id} not found.");

            return Result<UoMDto>.Success(new UoMDto { Id = uOM.Id, Name = uOM.Name });
        }

        public async Task<Result<UoMDto>> CreateUoMAsync(UoMCreateDto uomCreateDto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(uomCreateDto.Name))
                return Result<UoMDto>.Failure("UOM name cannot be empty.");

            var newUoM = new UoM
            {
                Name = uomCreateDto.Name
            };

            await _unitOfWork.UoMRepository.AddAsync(newUoM, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<UoMDto>.Success(new UoMDto { Id = newUoM.Id, Name = newUoM.Name });
        }

        public async Task<Result> DeleteUoMAsync(int id, CancellationToken cancellationToken)
        {
            var existedUOM = await _unitOfWork.UoMRepository.GetByIdAsync(id, cancellationToken);
            if (existedUOM == null)
                return Result.Failure($"UOM with ID {id} not found.");

            existedUOM.IsDeleted = true;
            await _unitOfWork.UoMRepository.UpdateAsync(existedUOM, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.UoMRepository.ExistsAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
            return Result<bool>.Success(exists);
        }

        public async Task<Result<UoMDto>> UpdateUoMAsync(int id, UoMUpdateDto uomUpdateDto, CancellationToken cancellationToken)
        {
            var existedUOM = await _unitOfWork.UoMRepository.GetByIdAsync(id, cancellationToken);

            if (existedUOM == null)
                return Result<UoMDto>.Failure($"UOM with ID {id} not found.");

            if(string.IsNullOrWhiteSpace(uomUpdateDto.Name))
                return Result<UoMDto>.Failure("UOM name cannot be empty.");

            existedUOM.Name = uomUpdateDto.Name;
            await _unitOfWork.UoMRepository.UpdateAsync(existedUOM, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<UoMDto>.Success(new UoMDto { Id = existedUOM.Id, Name = existedUOM.Name });
        }
    }
}



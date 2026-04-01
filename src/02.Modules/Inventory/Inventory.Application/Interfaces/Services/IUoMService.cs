using Inventory.Application.DTOs.UoMs;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services
{
    public interface IUoMService
    {
        Task<Result<List<UoMDto>>> GetAllUoMsAsync(CancellationToken cancellationToken);
        Task<Result<UoMDto>> GetUoMByIdAsync(int id, CancellationToken cancellationToken);
        Task<Result<UoMDto>> CreateUoMAsync(UoMCreateDto uomCreateDto, CancellationToken cancellationToken);
        Task<Result<UoMDto>> UpdateUoMAsync(int id, UoMUpdateDto uomUpdateDto, CancellationToken cancellationToken);
        Task<Result> DeleteUoMAsync(int id, CancellationToken cancellationToken);
        Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken);
    }
}




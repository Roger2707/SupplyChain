using ECommerce.Application.DTOs.Baskets;
using SharedKernel.Entities;

namespace ECommerce.Application.Interfaces.Services
{
    public interface IBasketService
    {
        Task<Result<BasketDto>> GetByBasketIdAsync(int basketId, CancellationToken cancellationToken = default);
        Task<Result<BasketDto>> GetByUserIdAsync(CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<Result<BasketDto>> IncreaseQuantity(UpsertBasketItem upsertBasketItem, CancellationToken cancellationToken = default);
        Task<Result<BasketDto>> DecreaseQuantity(UpsertBasketItem upsertBasketItem, CancellationToken cancellationToken = default);
        Task<Result<BasketDto>> ClearBasketAsync(CancellationToken cancellationToken = default);
    }
}

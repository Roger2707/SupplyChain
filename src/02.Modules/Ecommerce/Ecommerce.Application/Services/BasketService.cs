using ECommerce.Application.DTOs.Baskets;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities.Baskets;
using SharedKernel.Entities;
using SharedKernel.Interfaces;
using SharedKernel.Ultilities;

namespace ECommerce.Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryAdapterService _inventoryAdapterService;

        public BasketService(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentUserService currentUserService, IInventoryAdapterService inventoryAdapterService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
            _inventoryAdapterService = inventoryAdapterService;
        }

        #region GET

        public async Task<Result<BasketDto>> GetByBasketIdAsync(int basketId, CancellationToken cancellationToken = default)
        {
            var basket = await _unitOfWork.BasketRepository.GetByBasketIdAsync(basketId, cancellationToken);
            if (basket == null) return Result<BasketDto>.Failure($"Basket with ID {basketId} not found.");
            return Result<BasketDto>.Success(MapToDto(basket));
        }

        public async Task<Result<BasketDto>> GetByUserIdAsync(CancellationToken cancellationToken = default)
        {
            int userId = _currentUserService.UserId;
            if (CF.GetInt(userId) == 0)
                return Result<BasketDto>.Failure($"UserId : {userId} is invalid !");

            try
            {
                var cachedBasket = await GetCachedBasket(userId);
                if (cachedBasket == null)
                {
                    var basketDB = await _unitOfWork.BasketRepository.GetByUserIdAsync(userId, cancellationToken);
                    if (basketDB == null)
                    {
                        // Create new Basket (if user doesn't have basket)
                        basketDB = new Basket { UserId = userId };

                        await _unitOfWork.BasketRepository.AddAsync(basketDB, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    string key = CF.GetCachedBasketKey(userId);
                    await _cacheService.SetAsync<BasketDto>(key, MapToDto(basketDB));
                    return Result<BasketDto>.Success(MapToDto(basketDB));
                }
                return Result<BasketDto>.Success(cachedBasket);
            }
            catch(Exception ex)
            {
                return Result<BasketDto>.Failure(ex.Message);
            }
        }

        #endregion

        #region Cache Handlers

        private async Task<BasketDto> GetCachedBasket(int userId)
        {
            string key = CF.GetCachedBasketKey(userId);
            try
            {
                var basketDTO = await _cacheService.GetAsync<BasketDto>(key);
                if (basketDTO == null) return null;
                return basketDTO;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region CRUD

        public async Task<Result<BasketDto>> IncreaseQuantity(UpsertBasketItem upsertBasketItem, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await GetByUserIdAsync();
                if (!result.IsSuccess)
                    return Result<BasketDto>.Failure($"Some thing went wrong went get or create new basket ! Try again !");

                var basketDto = result.Data;
                var existedItem = basketDto.Items.FirstOrDefault(i => i.ProductId == upsertBasketItem.ProductId);
                var sellingPriceInfo = await _inventoryAdapterService.GetProductSellingPrice(upsertBasketItem.ProductId, cancellationToken);

                if (existedItem == null)
                {
                    var newItem = new BasketItemDto
                    {
                        ProductId = upsertBasketItem.ProductId,
                        Quantity = 1,
                        ProductName = sellingPriceInfo.Name,
                        UnitPrice = sellingPriceInfo.SellingPrice,
                    };
                    basketDto.Items.Add(newItem);
                }
                else
                {
                    existedItem.Quantity += 1;
                    existedItem.UnitPrice = sellingPriceInfo.SellingPrice; // Update UnitPrice in case it was changed in Inventory
                    existedItem.LineTotal = existedItem.Quantity * existedItem.UnitPrice; // Update LineTotal
                }

                string key = CF.GetCachedBasketKey(_currentUserService.UserId);
                await _cacheService.SetAsync<BasketDto>(key, basketDto);
                return Result<BasketDto>.Success(basketDto);
            }
            catch(Exception ex)
            {
                return Result<BasketDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<BasketDto>> DecreaseQuantity(UpsertBasketItem upsertBasketItem, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await GetByUserIdAsync();
                if (!result.IsSuccess)
                    return Result<BasketDto>.Failure($"Some thing went wrong went get or create new basket ! Try again !");

                var basketDto = result.Data;
                var existedItem = basketDto.Items.FirstOrDefault(i => i.ProductId == upsertBasketItem.ProductId);
                var sellingPriceInfo = await _inventoryAdapterService.GetProductSellingPrice(upsertBasketItem.ProductId, cancellationToken);

                if (existedItem == null)
                    return Result<BasketDto>.Failure($"ProductId {upsertBasketItem.ProductId} is not existed in Basket !");
                else
                {
                    existedItem.Quantity -= 1;
                    existedItem.UnitPrice = sellingPriceInfo.SellingPrice; // Update UnitPrice in case it was changed in Inventory
                    existedItem.LineTotal = existedItem.Quantity * existedItem.UnitPrice; // Update LineTotal

                    if (existedItem.Quantity == 0)
                        basketDto.Items.Remove(existedItem);
                }

                string key = CF.GetCachedBasketKey(_currentUserService.UserId);
                await _cacheService.SetAsync<BasketDto>(key, basketDto);
                return Result<BasketDto>.Success(basketDto);
            }
            catch (Exception ex)
            {
                return Result<BasketDto>.Failure(ex.Message);
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var basket = await _unitOfWork.BasketRepository.GetByIdAsync(id, cancellationToken);
            if (basket == null) return Result.Failure($"Basket with ID {id} not found.");
            basket.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<BasketDto>> ClearBasketAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await GetByUserIdAsync(cancellationToken);
                if (!result.IsSuccess)
                    return Result<BasketDto>.Failure($"Some thing went wrong went get or create new basket ! Try again !");

                var basketDto = result.Data;
                basketDto.Items.Clear();
                string key = CF.GetCachedBasketKey(_currentUserService.UserId);
                await _cacheService.SetAsync<BasketDto>(key, basketDto);
                return Result<BasketDto>.Success(basketDto);
            }
            catch (Exception ex)
            {
                return Result<BasketDto>.Failure(ex.Message);
            }
        }

        #endregion

        #region Helpers

        private static BasketDto MapToDto(Basket basket)
        {
            var items = basket.Items.Select(x => new BasketItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.Quantity * x.UnitPrice
            }).ToList();

            return new BasketDto
            {
                Id = basket.Id,
                UserId = basket.UserId,
                CreatedAt = basket.CreatedAt,
                UpdatedAt = basket.UpdatedAt,
                Items = items,
                TotalAmount = items.Sum(x => x.LineTotal)
            };
        }

        #endregion
    }
}

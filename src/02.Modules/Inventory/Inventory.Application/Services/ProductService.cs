using Inventory.Application.Common.Pagination;
using Inventory.Application.DTOs.Products;
using Inventory.Application.Interfaces.Generators;
using Inventory.Application.Interfaces.Queries;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SharedKernel.Entities;
using SharedKernel.Interfaces;
using SharedKernel.Ultilities;

namespace Inventory.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IProductQueries _productQueries;
        private readonly ISkuGenerator _skuGenerator;
        private readonly IBarcodeGenerator _barcodeGenerator;

        public ProductService(IUnitOfWork unitOfWork, ICacheService cacheService, IProductQueries productQueries, ISkuGenerator skuGenerator, IBarcodeGenerator barcodeGenerator)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _productQueries = productQueries;
            _skuGenerator = skuGenerator;
            _barcodeGenerator = barcodeGenerator;
        }

        #region GETs

        public async Task<Result<List<ProductDto>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var products = await _productQueries.GetAllAsync(cancellationToken);
            return Result<List<ProductDto>>.Success(products.ToList());
        }

        public async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var product = await _productQueries.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Failure($"Product with ID {id} not found.");
            }
            return Result<ProductDto>.Success(product);
        }

        public async Task<Result<PagedResult<ProductDto>>> GetProductsPagedAsync(ProductParams param, CancellationToken cancellationToken)
        {
            var productPaged = await _unitOfWork.ProductRepository.GetProductsPagedAsync(param);
            return Result<PagedResult<ProductDto>>.Success(productPaged);
        }

        #endregion

        #region CRUDs

        public async Task<Result<ProductDto>> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken)
        {
            if(!IsDataValid(createProductDto, out var validationMessage))
            {
                return Result<ProductDto>.Failure(validationMessage);
            }
            var sku = await _skuGenerator.GenerateAsync(cancellationToken);
            var barcode = await _barcodeGenerator.GenerateAsync(cancellationToken);

            var newProduct = new Product
            {
                Name = createProductDto.Name,
                SKU = sku,
                Barcode = barcode,
                CategoryId = createProductDto.CategoryId,
                BaseUoMId = createProductDto.BaseUoMId,
                MinStockLevel = createProductDto.MinStockLevel,
            };

            if(createProductDto.ConversionDtos.Any())
            {
                var conversions = createProductDto.ConversionDtos
                .Select(c => new ProductUoMConversion
                {
                    ProductId = newProduct.Id,
                    FromUoMId = c.FromUoMId,
                    ToUoMId = c.ToUoMId,
                    Factor = c.Factor
                })
                .ToList();
                newProduct.Conversions = conversions;
            }

            await _unitOfWork.ProductRepository.AddAsync(newProduct, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = MapToDto(newProduct);
            return Result<ProductDto>.Success(productDto);
        }

        public async Task<Result<ProductDto>> UpdateAsync(int id, UpdateProductDto updateProductDto, CancellationToken cancellationToken)
        {
            if(!IsDataValid(updateProductDto, out var validationMessage))
            {
                return Result<ProductDto>.Failure(validationMessage);
            }
            var existedProduct = await _unitOfWork.ProductRepository.GetWithConversionAsync(id, cancellationToken);

            if(existedProduct == null)
            {
                return Result<ProductDto>.Failure($"Product with ID {id} not found.");
            }

            if (updateProductDto.RowVersion != null && existedProduct.RowVersion != null)
            {
                existedProduct.RowVersion = updateProductDto.RowVersion;
            }

            existedProduct.Name = updateProductDto.Name;
            existedProduct.CategoryId = updateProductDto.CategoryId;
            existedProduct.BaseUoMId = updateProductDto.BaseUoMId;
            existedProduct.MinStockLevel = updateProductDto.MinStockLevel;

            existedProduct.Conversions.Clear();
            foreach (var dto in updateProductDto.ConversionDtos)
            {
                existedProduct.Conversions.Add(new ProductUoMConversion
                {
                    ProductId = existedProduct.Id,
                    FromUoMId = dto.FromUoMId,
                    ToUoMId = dto.ToUoMId,
                    Factor = dto.Factor
                });
            }
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<ProductDto>.Failure("Product is updated by other users, please update again !");
            }

            var productDto = MapToDto(existedProduct);
            return Result<ProductDto>.Success(productDto);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existed = await _unitOfWork.ProductRepository.GetByIdAsync(id, cancellationToken);
            if(existed == null)
            {
                return Result.Failure($"Product with ID {id} not found.");
            }
            existed.IsDeleted = true;
            await _unitOfWork.ProductRepository.UpdateAsync(existed, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken)
        {
            var exist = await _unitOfWork.ProductRepository.ExistsAsync(p => p.Id == id, cancellationToken);
            return Result<bool>.Success(exist);
        }

        #endregion

        #region Business Rules

        public async Task SetProductSellingPrice(int productId, CancellationToken cancellationToken)
        {
            Product product = await _unitOfWork.ProductRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new Exception($"Product with ID {productId} not found.");

            var layers = await _unitOfWork.InventoryCostLayerRepository.GetLayersByProductId(productId, cancellationToken);
            if(layers == null || layers.Count == 0)
                throw new Exception($"Product with ID {productId} not found in Inventory.");

            var avgCost = layers.Sum(l => l.UnitCost * l.OriginalQty) / layers.Sum(l => l.OriginalQty);
            var sellingPrice = Math.Round(avgCost * CF.Margin, 2);

            product.SellingPrice = sellingPrice;
        }

        public Task<List<Product>> GetProductsByIdsAsync(List<int> productIds, CancellationToken cancellationToken)
        {
            return _unitOfWork.ProductRepository.GetProductsByIds(productIds, cancellationToken);
        }

        public async Task<Dictionary<int, ProductSellingPrice>> GetProductsSellingPrice(List<int> productIds, CancellationToken cancellationToken = default)
        {
            var productsSellingPrice = new Dictionary<int, ProductSellingPrice>();
            foreach (int productId in productIds)
            {
                var cacheKey = CF.GetCachedProductSellingPriceKey(productId);
                var cacheSellingPriceItem = await _cacheService.GetAsync<ProductSellingPrice>(cacheKey);
                if (cacheSellingPriceItem != null)
                    productsSellingPrice[productId] = cacheSellingPriceItem;
                else
                    productsSellingPrice[productId] = null; // Mark as null to indicate not found in cache, we will get from DB later
            }

            foreach (var kvp in productsSellingPrice)
            {
                if (kvp.Value == null) // Not found in cache, get from DB
                {
                    var product = await _unitOfWork.ProductRepository.GetByIdAsync(kvp.Key, cancellationToken);
                    if (product != null)
                    {
                        var sellingPriceInfo = new ProductSellingPrice
                        {
                            Id = product.Id,
                            Name = product.Name,
                            SellingPrice = product.SellingPrice
                        };
                        productsSellingPrice[kvp.Key] = sellingPriceInfo;

                        // Set to cache for future use
                        await _cacheService.SetAsync<ProductSellingPrice>(CF.GetCachedProductSellingPriceKey(kvp.Key), sellingPriceInfo);
                    }
                    else
                        throw new Exception($"Product ID {kvp.Key} is not Existed !");
                }
            }

            return productsSellingPrice;
        }

        public async Task<ProductSellingPrice> GetProductSellingPrice(int productId, CancellationToken cancellationToken = default)
        {
            var cacheKey = CF.GetCachedProductSellingPriceKey(productId);
            var cacheSellingPriceItem = await _cacheService.GetAsync<ProductSellingPrice>(cacheKey);
            if (cacheSellingPriceItem != null)
                return cacheSellingPriceItem;

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new Exception($"Product ID {productId} is not Existed !");
            var sellingPriceInfo = new ProductSellingPrice
            {
                Id = product.Id,
                Name = product.Name,
                SellingPrice = product.SellingPrice
            };
            // Set to cache for future use
            await _cacheService.SetAsync<ProductSellingPrice>(CF.GetCachedProductSellingPriceKey(productId), sellingPriceInfo);
            return sellingPriceInfo;
        }

        #endregion

        #region Helpers

        private bool IsDataValid(CreateProductDto createProductDto, out string message)
        {
            if (string.IsNullOrWhiteSpace(createProductDto.Name))
            {
                message = "Product name is required.";
                return false;
            }
            if (createProductDto.MinStockLevel < 0)
            {
                message = "Min stock level cannot be negative.";
                return false;
            }
            if (createProductDto.CategoryId <= 0)
            {
                message = "Valid category ID is required.";
                return false;
            }
            if (createProductDto.BaseUoMId <= 0)
            {
                message = "Valid base UoM ID is required.";
                return false;
            }
            message = string.Empty;
            return true;
        }

        private bool IsDataValid(UpdateProductDto updateProductDto, out string message)
        {
            if (string.IsNullOrWhiteSpace(updateProductDto.Name))
            {
                message = "Product name is required.";
                return false;
            }
            if (updateProductDto.MinStockLevel < 0)
            {
                message = "Min stock level cannot be negative.";
                return false;
            }
            if (updateProductDto.CategoryId <= 0)
            {
                message = "Valid category ID is required.";
                return false;
            }
            if (updateProductDto.BaseUoMId <= 0)
            {
                message = "Valid base UoM ID is required.";
                return false;
            }
            message = string.Empty;
            return true;
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                BaseUoMId = product.BaseUoMId,
                MinStockLevel = product.MinStockLevel,
                SellingPrice = product.SellingPrice,
                IsPerishable = product.IsPerishable,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                RowVersion = product.RowVersion
            };
        }

        #endregion
    }
}



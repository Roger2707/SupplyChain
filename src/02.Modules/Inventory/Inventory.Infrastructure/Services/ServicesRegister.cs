using SharedKernel.Repositories;
using Inventory.Application.Interfaces;
using Inventory.Application.Interfaces.Generators;
using Inventory.Application.Interfaces.Queries;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Application.Services;
using Inventory.Infrastructure.Queries;
using Inventory.Infrastructure.Repositories;
using Inventory.Infrastructure.Repositories.Generators;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.Services
{
    public static class ServicesRegister
    {
        public static void AddInventoryServices(this IServiceCollection services)
        {
            // Register unit of work
            // Note: Repositories are created by UnitOfWork directly (not through DI)
            // This ensures all repositories use the same DbContext instance as UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ISkuGenerator, SKUGenerator>();
            services.AddScoped<IBarcodeGenerator, BarcodeGenerator>();
            services.AddScoped<IPurchaseOrderGenerator, PurchaseOrderGenerator>();
            services.AddScoped<IGoodsReceiptGenerator, GoodsReceiptGenerator>();
            services.AddScoped<ISalesOrderGenerator, SalesOrderGenerator>();
            services.AddScoped<IDeliveryGenerator, DeliveryGenerator>();
            services.AddScoped<IInvoiceGenerator, InvoiceGenerator>();
            services.AddScoped<IWarehouseService, WarehouseService>();

            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUoMService, UoMService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
            services.AddScoped<IStockTransferService, StockTransferService>();
            services.AddScoped<IInventoryLedgerService, InventoryLedgerService>();
            services.AddScoped<IInventoryCostLayerService, InventoryCostLayerService>();
            services.AddScoped<ISupplierProductPriceService, SupplierProductPriceService>();
            services.AddScoped<ISalesOrderService, SalesOrderService>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IReportService, ReportService>();

            services.AddScoped<IInventoryService, InventoryService>();

            // Register Dapper query services (read-model)
            services.AddScoped<IDapperExecutor, DapperExecutor>();

            services.AddScoped<ICategoryQueries, CategoryQueries>();
            services.AddScoped<IProductQueries, ProductQueries>();
            services.AddScoped<IReportQueries, ReportQueries>();
        }
    }
}

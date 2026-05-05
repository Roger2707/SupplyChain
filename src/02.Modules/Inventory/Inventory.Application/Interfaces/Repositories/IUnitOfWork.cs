namespace Inventory.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    IRepository<T> GetRepository<T>() where T : class;
    IWarehouseRepository WarehouseRepository { get; }
    ICustomerRepository CustomerRepository { get; }
    ISupplierRepository SupplierRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IUoMRepository UoMRepository { get; }
    IProductRepository ProductRepository { get; }
    IPurchaseOrderRepository PurchaseOrderRepository { get; }
    IGoodsReceiptRepository GoodsReceiptRepository { get; }
    IStockTransferRepository StockTransferRepository { get; }
    IInventoryLedgerRepository InventoryLedgerRepository { get; }
    IInventoryCostLayerRepository InventoryCostLayerRepository { get; }
    ISupplierProductPriceRepository SupplierProductPriceRepository { get; }
    ISalesOrderRepository SalesOrderRepository { get; }
    IDeliveryRepository DeliveryRepository { get; }
    IInventoryReservationRepository InventoryReservationRepository { get; }
    IInvoiceRepository InvoiceRepository { get; }
    IJournalEntryRepository JournalEntryRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}


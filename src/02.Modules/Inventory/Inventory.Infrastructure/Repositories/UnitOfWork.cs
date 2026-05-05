using Inventory.Application.Interfaces.Repositories;
using Inventory.Infrastructure.Data;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Repositories;

public class UnitOfWork : EfUnitOfWorkBase<InventoryDbContext>, IUnitOfWork
{
    private readonly InventoryDbContext _context;
    private readonly Dictionary<Type, object> _repositories;

    private IWarehouseRepository? _warehouseRepository;
    private ICustomerRepository? _customerRepository;
    private ISupplierRepository? _supplierRepository;
    private ICategoryRepository? _categoryRepository;
    private IUoMRepository? _uomRepository;
    private IProductRepository? _productRepository;
    private IPurchaseOrderRepository? _purchaseOrderRepository;
    private IGoodsReceiptRepository? _goodsReceiptRepository;
    private IStockTransferRepository? _stockTransferRepository;
    private IInventoryLedgerRepository? _inventoryLedgerRepository;
    private IInventoryCostLayerRepository? _inventoryCostLayerRepository;
    private ISupplierProductPriceRepository? _supplierProductPriceRepository;
    private ISalesOrderRepository? _salesOrderRepository;
    private IDeliveryRepository? _deliveryRepository;
    private IInventoryReservationRepository? _inventoryReservationRepository;
    private IInvoiceRepository? _invoiceRepository;
    private IJournalEntryRepository? _journalEntryRepository;

    public UnitOfWork(InventoryDbContext context)
        : base(context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    #region Repository Accessors

    public IRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T);
        
        if (_repositories.ContainsKey(type))
        {
            return (IRepository<T>)_repositories[type];
        }

        var repository = new Repository<T>(_context);
        _repositories[type] = repository;
        return repository;
    }

    public IWarehouseRepository WarehouseRepository
    {
        get
        {
            if (_warehouseRepository == null)
            {
                _warehouseRepository = new WarehouseRepository(_context);
            }
            return _warehouseRepository;
        }
    }

    public ICustomerRepository CustomerRepository
    {
        get
        {
            if (_customerRepository == null)
            {
                _customerRepository = new CustomerRepository(_context);
            }
            return _customerRepository;
        }
    }

    public ISupplierRepository SupplierRepository
    {
        get
        {
            if (_supplierRepository == null)
            {
                _supplierRepository = new SupplierRepository(_context);
            }
            return _supplierRepository;
        }
    }

    public ICategoryRepository CategoryRepository
    {
        get
        {
            if (_categoryRepository == null)
            {
                _categoryRepository = new CategoryRepository(_context);
            }
            return _categoryRepository;
        }
    }

    public IUoMRepository UoMRepository
    {
        get
        {
            if (_uomRepository == null)
            {
                _uomRepository = new UoMRepository(_context);
            }
            return _uomRepository;
        }
    }

    public IProductRepository ProductRepository
    {
        get
        {
            if (_productRepository == null)
            {
                _productRepository = new ProductRepository(_context);
            }
            return _productRepository;
        }
    }

    public IPurchaseOrderRepository PurchaseOrderRepository
    {
        get
        {
            if (_purchaseOrderRepository == null)
            {
                _purchaseOrderRepository = new PurchaseOrderRepository(_context);
            }
            return _purchaseOrderRepository;
        }
    }

    public IGoodsReceiptRepository GoodsReceiptRepository
    {
        get
        {
            if (_goodsReceiptRepository == null)
            {
                _goodsReceiptRepository = new GoodsReceiptRepository(_context);
            }
            return _goodsReceiptRepository;
        }
    }

    public IStockTransferRepository StockTransferRepository
    {
        get
        {
            if (_stockTransferRepository == null)
            {
                _stockTransferRepository = new StockTransferRepository(_context);
            }
            return _stockTransferRepository;
        }
    }

    public IInventoryLedgerRepository InventoryLedgerRepository
    {
        get
        {
            if (_inventoryLedgerRepository == null)
            {
                _inventoryLedgerRepository = new InventoryLedgerRepository(_context);
            }
            return _inventoryLedgerRepository;
        }
    }

    public IInventoryCostLayerRepository InventoryCostLayerRepository
    {
        get
        {
            if (_inventoryCostLayerRepository == null)
            {
                _inventoryCostLayerRepository = new InventoryCostLayerRepository(_context);
            }
            return _inventoryCostLayerRepository;
        }
    }

    public ISupplierProductPriceRepository SupplierProductPriceRepository
    {
        get
        {
            if (_supplierProductPriceRepository == null)
            {
                _supplierProductPriceRepository = new SupplierProductPriceRepository(_context);
            }
            return _supplierProductPriceRepository;
        }
    }

    public ISalesOrderRepository SalesOrderRepository
    {
        get
        {
            if (_salesOrderRepository == null)
            {
                _salesOrderRepository = new SalesOrderRepository(_context);
            }
            return _salesOrderRepository;
        }
    }

    public IDeliveryRepository DeliveryRepository
    {
        get
        {
            if (_deliveryRepository == null)
            {
                _deliveryRepository = new DeliveryRepository(_context);
            }
            return _deliveryRepository;
        }
    }

    public IInventoryReservationRepository InventoryReservationRepository
    {
        get
        {
            if (_inventoryReservationRepository == null)
            {
                _inventoryReservationRepository = new InventoryReservationRepository(_context);
            }
            return _inventoryReservationRepository;
        }
    }

    public IInvoiceRepository InvoiceRepository
    {
        get
        {
            if (_invoiceRepository == null)
            {
                _invoiceRepository = new InvoiceRepository(_context);
            }
            return _invoiceRepository;
        }
    }

    public IJournalEntryRepository JournalEntryRepository
    {
        get
        {
            if (_journalEntryRepository == null)
            {
                _journalEntryRepository = new JournalEntryRepository(_context);
            }
            return _journalEntryRepository;
        }
    }

    #endregion

}





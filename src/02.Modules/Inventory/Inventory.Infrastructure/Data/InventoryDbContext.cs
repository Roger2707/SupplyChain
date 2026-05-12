using Inventory.Domain.Entities;
using Inventory.Domain.Entities.Accounts;
using Inventory.Domain.Entities.Delivery;
using Inventory.Domain.Entities.GoodsReceipt;
using Inventory.Domain.Entities.Inventory;
using Inventory.Domain.Entities.Invoice;
using Inventory.Domain.Entities.Products;
using Inventory.Domain.Entities.PurchaseOrder;
using Inventory.Domain.Entities.SalesOrder;
using Inventory.Domain.Entities.StockTransfer;
using Inventory.Domain.Entities.Suppliers;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public DbSet<Warehouse> Warehouses { get; set; }

    // Customer / Supplier entities would go here
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }

    // Category entities
    public DbSet<Category> Categories { get; set; }

    // Product entities
    public DbSet<UoM> UoMs { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductUoMConversion> ProductUoMConversions { get; set; }

    // Purchase Order entities
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    // Goods Receipt entities
    public DbSet<GoodsReceipt> GoodsReceipts { get; set; }

    // Stock Transfer entities
    public DbSet<StockTransfer> StockTransfers { get; set; }

    // Inventory entities
    public DbSet<InventoryLedger> InventoryLedgers { get; set; }
    public DbSet<InventoryCostLayer> InventoryCostLayers { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }

    // Supplier pricing
    public DbSet<SupplierProductPrice> SupplierProductPrices { get; set; }

    // Sales Order
    public DbSet<SalesOrder> SalesOrders { get; set; }

    // Delivery
    public DbSet<Delivery> Deliveries { get; set; }

    // Invoice
    public DbSet<Invoice> Invoices { get; set; }

    // Account
    public DbSet<Account> Accounts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }

    // OutBox Saga
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<OutboxState> OutboxStates { get; set; }
    public DbSet<InboxState> InboxStates { get; set; }

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define a sequence for Product SKU generation
        modelBuilder.HasSequence<int>("ProductSkuSequence")
                    .StartsAt(1)
                    .IncrementsBy(1);

        // Define a sequence for Product Barcode generation
        modelBuilder.HasSequence<int>("ProductBarcodeSequence")
                    .StartsAt(1)
                    .IncrementsBy(1);

        modelBuilder.HasSequence<int>("PurchaseOrderSequence")
                    .StartsAt(10)
                    .IncrementsBy(1);

        modelBuilder.HasSequence<int>("GoodsReceiptSequence")
                    .StartsAt(10)
                    .IncrementsBy(1);

        modelBuilder.HasSequence<int>("SalesOrderSequence")
                    .StartsAt(10)
                    .IncrementsBy(1);

        modelBuilder.HasSequence<int>("DeliverySequence")
                    .StartsAt(10)
                    .IncrementsBy(1);

        modelBuilder.HasSequence<int>("InvoiceSequence")
            .StartsAt(1)
            .IncrementsBy(1);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        
        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }

        // Configurations for Outbox - MassTransit
        modelBuilder.AddInboxStateEntity(e => e.ToTable("InboxState", "inventory"));
        modelBuilder.AddOutboxMessageEntity(e => e.ToTable("OutboxMessage", "inventory"));
        modelBuilder.AddOutboxStateEntity(e => e.ToTable("OutboxState", "inventory"));
    }

    private static System.Linq.Expressions.LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}


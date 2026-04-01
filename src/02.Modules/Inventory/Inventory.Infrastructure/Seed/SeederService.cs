using Inventory.Application.DTOs.Delivery;
using Inventory.Application.DTOs.GoodsReceipts;
using Inventory.Application.DTOs.Invoices;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities;
using Inventory.Domain.Entities.Accounts;
using Inventory.Domain.Entities.Products;
using Inventory.Domain.Entities.PurchaseOrder;
using Inventory.Domain.Entities.SalesOrder;
using Inventory.Domain.Entities.Suppliers;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SharedKernel.Interfaces;
using SharedKernel.Ultilities;

namespace Inventory.Infrastructure.Seed
{
    public class SeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoodsReceiptService _goodsReceiptService;
        private readonly ISalesOrderService _salesOrderService;
        private readonly IDeliveryService _deliveryService;
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;
        private readonly ICacheService _cacheService;

        public SeederService(ApplicationDbContext context, IUnitOfWork unitOfWork, IGoodsReceiptService goodsReceiptService, ISalesOrderService salesOrderService, IDeliveryService deliveryService, IInvoiceService invoiceService, IProductService productService, ICacheService cacheService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _goodsReceiptService = goodsReceiptService;
            _salesOrderService = salesOrderService;
            _deliveryService = deliveryService;
            _invoiceService = invoiceService;
            _productService = productService;
            _cacheService = cacheService;
        }

        public async Task SeedDataAsync()
        {
            await SeedBaseDataAsync();

            await SeedFlowDataAsync();

            await SeedProductSellingPriceCacheAsync();
        }

        public async Task SeedBaseDataAsync()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (!await _context.Accounts.AnyAsync())
                    await SeedAccoutsAsync();

                if (!await _context.Regions.AnyAsync())
                    await SeedRegionAsync();

                if (!await _context.Warehouses.AnyAsync())
                    await SeedWarehouseAsync();

                if (!await _context.Customers.AnyAsync())
                    await SeedCustomersAsync();

                if (!await _context.Suppliers.AnyAsync())
                    await SeedSuppliersAsync();

                if (!await _context.Categories.AnyAsync())
                    await SeedCategoriesAsync();

                if (!await _context.UoMs.AnyAsync())
                    await SeedUoMsAsync();

                if (!await _context.Products.AnyAsync())
                    await SeedProductsAsync();

                if (await _context.Products.AnyAsync() && await _context.UoMs.AnyAsync() && !await _context.ProductUoMConversions.AnyAsync())
                    await SeedProductConversionsAsync();

                if (!await _context.SupplierProductPrices.AnyAsync())
                    await SeedSupplierProductPricesAsync();

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync( );
            }
            catch
            {
                await _unitOfWork?.RollbackTransactionAsync();
                return;
            }
        }

        #region Base Data

        private async Task SeedAccoutsAsync()
        {
            var accounts = new List<Account>
            {
                new Account
                {
                    Code = "1000",
                    Name = "Cash",
                    Type = AccountType.Asset,
                    IsActive = true
                },
                new Account
                {
                    Code = "1100",
                    Name = "Accounts Receivable",
                    Type = AccountType.Asset,
                    IsActive = true
                },
                new Account
                {
                    Code = "1200",
                    Name = "Inventory",
                    Type = AccountType.Asset,
                    IsActive = true
                },
                new Account
                {
                    Code = "4000",
                    Name = "Revenue",
                    Type = AccountType.Revenue,
                    IsActive = true
                },
                new Account
                {
                    Code = "5000",
                    Name = "Cost Of Goods Sold",
                    Type = AccountType.Expense,
                    IsActive = true
                }
            };

            foreach (var acc in accounts)
                _context.Accounts.Add(acc);

            await _context.SaveChangesAsync();
        }

        private async Task SeedRegionAsync()
        {
            var regions = new List<Region>
            {
                new Region {RegionCode = "RE - 001", RegionName = "South"},
                new Region {RegionCode = "RE - 002", RegionName = "North"},
                new Region {RegionCode = "RE - 003", RegionName = "Central"},
                new Region {RegionCode = "RE - 004", RegionName = "International"},
            };

            foreach (var region in regions)
                _context.Regions.Add(region);

            await _context.SaveChangesAsync();
        }

        private async Task SeedWarehouseAsync()
        {
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    WarehouseCode = "WH - 001",
                    WarehouseName = "Warehouse HCM Base",
                    Address = "01 Le Duan, P.Ben Thanh, HCMC",
                    Description = "HCM Warehouse",
                    PhoneNumber = "1234567890",
                    RegionId = 1,
                },
                new Warehouse
                {
                    WarehouseCode = "WH - 002",
                    WarehouseName = "Warehouse HN Base",
                    Address = "01 Ho Xuan Huong, HN",
                    Description = "HN Warehouse",
                    PhoneNumber = "1234567890",
                    RegionId = 2,
                },
                new Warehouse
                {
                    WarehouseCode = "WH - 003",
                    WarehouseName = "Warehouse VT Base",
                    Address = "01 Hoang Hoa Tham, P.Vung Tau, HCMC",
                    Description = "VT Warehouse",
                    PhoneNumber = "1234567890",
                    RegionId = 1,
                },
            };

            foreach (var warehouse in warehouses)
                _context.Warehouses.Add(warehouse);

            await _context.SaveChangesAsync();
        }

        private async Task SeedCustomersAsync()
        {
            var customers = new List<Customer>
            {
                new Customer { CustomerCode = "CUS - 001", CustomerName = "Phoenix Dynamics", Address = "New York", PhoneNumber = "0900000001", Description = "Strategic Partner" },
                new Customer { CustomerCode = "CUS - 002", CustomerName = "Silverline Holdings", Address = "Los Angeles", PhoneNumber = "0900000002", Description = "Premium Client" },
                new Customer { CustomerCode = "CUS - 003", CustomerName = "NovaEdge Solutions", Address = "Chicago", PhoneNumber = "0900000003" },
                new Customer { CustomerCode = "CUS - 004", CustomerName = "Ironclad Ventures", Address = "Houston", PhoneNumber = "0900000004" },
                new Customer { CustomerCode = "CUS - 005", CustomerName = "BluePeak Industries", Address = "Seattle", PhoneNumber = "0900000005" },
                new Customer { CustomerCode = "CUS - 006", CustomerName = "Quantum Axis Corp", Address = "San Francisco", PhoneNumber = "0900000006" },
                new Customer { CustomerCode = "CUS - 007", CustomerName = "Velocity Group", Address = "Boston", PhoneNumber = "0900000007" },
                new Customer { CustomerCode = "CUS - 008", CustomerName = "Apex Horizon Ltd", Address = "Denver", PhoneNumber = "0900000008" },
                new Customer { CustomerCode = "CUS - 009", CustomerName = "TitanCore Enterprises", Address = "Miami", PhoneNumber = "0900000009" },
                new Customer { CustomerCode = "CUS - 010", CustomerName = "Eclipse Innovations", Address = "Atlanta", PhoneNumber = "0900000010" }
            };

            foreach (var c in customers)
                _context.Customers.Add(c);

            await _context.SaveChangesAsync();
        }

        private async Task SeedSuppliersAsync()
        {
            var suppliers = new List<Supplier>
            {
                new Supplier { SupplierCode = "SUP - 001", SupplierName = "BlackForge Supply Co.", Address = "New York", PhoneNumber = "0911000001", Description = "Primary Materials Provider" },
                new Supplier { SupplierCode = "SUP - 002", SupplierName = "StormFront Logistics", Address = "Los Angeles", PhoneNumber = "0911000002" },
                new Supplier { SupplierCode = "SUP - 003", SupplierName = "IronPeak Distribution", Address = "Chicago", PhoneNumber = "0911000003" },
                new Supplier { SupplierCode = "SUP - 004", SupplierName = "Vanguard Industrial", Address = "Houston", PhoneNumber = "0911000004" },
                new Supplier { SupplierCode = "SUP - 005", SupplierName = "Titan Supply Chain", Address = "Seattle", PhoneNumber = "0911000005" },
                new Supplier { SupplierCode = "SUP - 006", SupplierName = "DarkMatter Exports", Address = "San Francisco", PhoneNumber = "0911000006" },
                new Supplier { SupplierCode = "SUP - 007", SupplierName = "QuantumTrade Global", Address = "Boston", PhoneNumber = "0911000007" },
                new Supplier { SupplierCode = "SUP - 008", SupplierName = "SilverStone Manufacturing", Address = "Denver", PhoneNumber = "0911000008" },
                new Supplier { SupplierCode = "SUP - 009", SupplierName = "CrimsonLine Wholesale", Address = "Miami", PhoneNumber = "0911000009" },
                new Supplier { SupplierCode = "SUP - 010", SupplierName = "Nebula Industrial Group", Address = "Atlanta", PhoneNumber = "0911000010" }
            };

            foreach (var c in suppliers)
                _context.Suppliers.Add(c);

            await _context.SaveChangesAsync();
        }

        private async Task SeedCategoriesAsync()
        {
            if (_context.Categories.Any())
                return;

            // ===== ROOT LEVEL =====
            var ingredients = new Category { Name = "Ingredients" };
            var beverages = new Category { Name = "Beverages" };
            var kitchenSupplies = new Category { Name = "Kitchen Supplies" };

            _context.Categories.AddRange(ingredients, beverages, kitchenSupplies);
            await _context.SaveChangesAsync(); // Generate Ids


            // ===== LEVEL 2 =====
            var meat = new Category { Name = "Meat", ParentId = ingredients.Id };
            var seafood = new Category { Name = "Seafood", ParentId = ingredients.Id };
            var vegetables = new Category { Name = "Vegetables", ParentId = ingredients.Id };
            var dairy = new Category { Name = "Dairy", ParentId = ingredients.Id };

            var wine = new Category { Name = "Wine", ParentId = beverages.Id };
            var beer = new Category { Name = "Beer", ParentId = beverages.Id };
            var softDrinks = new Category { Name = "Soft Drinks", ParentId = beverages.Id };

            var spices = new Category { Name = "Spices", ParentId = kitchenSupplies.Id };
            var sauces = new Category { Name = "Sauces", ParentId = kitchenSupplies.Id };

            _context.Categories.AddRange(
                meat, seafood, vegetables, dairy,
                wine, beer, softDrinks,
                spices, sauces
            );

            await _context.SaveChangesAsync();

            // ===== LEVEL 3 =====
            var beef = new Category { Name = "Beef", ParentId = meat.Id };
            var chicken = new Category { Name = "Chicken", ParentId = meat.Id };

            var redWine = new Category { Name = "Red Wine", ParentId = wine.Id };
            var whiteWine = new Category { Name = "White Wine", ParentId = wine.Id };

            var importedBeer = new Category { Name = "Imported Beer", ParentId = beer.Id };

            _context.Categories.AddRange(
                beef, chicken,
                redWine, whiteWine,
                importedBeer
            );

            await _context.SaveChangesAsync();
        }

        private async Task SeedUoMsAsync()
        {
            if (_context.UoMs.Any()) return;

            var uoms = new List<UoM>
            {
                new UoM { Name = "Kilogram" },
                new UoM { Name = "Gram" },
                new UoM { Name = "Liter" },
                new UoM { Name = "Milliliter" },
                new UoM { Name = "Bottle" },
                new UoM { Name = "Case" },
                new UoM { Name = "Piece" }
            };

            _context.UoMs.AddRange(uoms);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProductsAsync()
        {
            if (_context.Products.Any()) return;

            var kg = await _context.UoMs.FirstAsync(x => x.Name == "Kilogram");
            var liter = await _context.UoMs.FirstAsync(x => x.Name == "Liter");
            var bottle = await _context.UoMs.FirstAsync(x => x.Name == "Bottle");

            var beef = await _context.Categories.FirstAsync(x => x.Name == "Beef");
            var chicken = await _context.Categories.FirstAsync(x => x.Name == "Chicken");
            var seafood = await _context.Categories.FirstAsync(x => x.Name == "Seafood");
            var vegetables = await _context.Categories.FirstAsync(x => x.Name == "Vegetables");
            var dairy = await _context.Categories.FirstAsync(x => x.Name == "Dairy");
            var redWine = await _context.Categories.FirstAsync(x => x.Name == "Red Wine");
            var beer = await _context.Categories.FirstAsync(x => x.Name == "Imported Beer");
            var softDrink = await _context.Categories.FirstAsync(x => x.Name == "Soft Drinks");
            var spices = await _context.Categories.FirstAsync(x => x.Name == "Spices");
            var sauces = await _context.Categories.FirstAsync(x => x.Name == "Sauces");

            var products = new List<Product>
            {
                new Product { Name="Beef Tenderloin", SKU="BF001", Barcode="8931234000017", CategoryId=beef.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=5, IsPerishable=true },
                new Product { Name="Ribeye Steak", SKU="BF002", Barcode="8931234000024", CategoryId=beef.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=5, IsPerishable=true },

                new Product { Name="Chicken Breast", SKU="CK001", Barcode="8931234000031", CategoryId=chicken.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=10, IsPerishable=true },
                new Product { Name="Whole Chicken", SKU="CK002", Barcode="8931234000048", CategoryId=chicken.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=8, IsPerishable=true },

                new Product { Name="Salmon Fillet", SKU="SF001", Barcode="8931234000055", CategoryId=seafood.Id, BaseUoMId=kg.Id, SellingPrice=5200000, MinStockLevel=5, IsPerishable=true },
                new Product { Name="Shrimp", SKU="SF002", Barcode="8931234000062", CategoryId=seafood.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=5, IsPerishable=true },

                new Product { Name="Broccoli", SKU="VG001", Barcode="8931234000079", CategoryId=vegetables.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=5, IsPerishable=true },
                new Product { Name="Carrot", SKU="VG002", Barcode="8931234000086", CategoryId=vegetables.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=5, IsPerishable=true },
                new Product { Name="Onion", SKU="VG003", Barcode="8931234000093", CategoryId=vegetables.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=10, IsPerishable=true },

                new Product { Name="Milk", SKU="DY001", Barcode="8931234000109", CategoryId=dairy.Id, BaseUoMId=liter.Id, SellingPrice=0, MinStockLevel=20, IsPerishable=true },
                new Product { Name="Cheddar Cheese", SKU="DY002", Barcode="8931234000116", CategoryId=dairy.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=5, IsPerishable=true },
                new Product { Name="Butter", SKU="DY003", Barcode="8931234000123", CategoryId=dairy.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=3, IsPerishable=true },

                new Product { Name="Cabernet Sauvignon", SKU="RW001", Barcode="8931234000130", CategoryId=redWine.Id, BaseUoMId=bottle.Id, SellingPrice=0, MinStockLevel=24, IsPerishable=false },
                new Product { Name="Merlot", SKU="RW002", Barcode="8931234000147", CategoryId=redWine.Id, BaseUoMId=bottle.Id, SellingPrice=0, MinStockLevel=24, IsPerishable=false },

                new Product { Name="Imported Lager Beer", SKU="BR001", Barcode="8931234000154", CategoryId=beer.Id, BaseUoMId=bottle.Id, SellingPrice=0, MinStockLevel=48, IsPerishable=false },
                new Product { Name="Coca Cola", SKU="SD001", Barcode="8931234000161", CategoryId=softDrink.Id, BaseUoMId=bottle.Id, SellingPrice=0, MinStockLevel=48, IsPerishable=false },

                new Product { Name="Black Pepper", SKU="SP001", Barcode="8931234000178", CategoryId=spices.Id, BaseUoMId=kg.Id, SellingPrice=3200000, MinStockLevel=1, IsPerishable=false },
                new Product { Name="Salt", SKU="SP002", Barcode="8931234000185", CategoryId=spices.Id, BaseUoMId=kg.Id, SellingPrice=0, MinStockLevel=5, IsPerishable=false },

                new Product { Name="Olive Oil", SKU="SC001", Barcode="8931234000192", CategoryId=sauces.Id, BaseUoMId=liter.Id, SellingPrice=0, MinStockLevel=10, IsPerishable=false },
                new Product { Name="Tomato Sauce", SKU="SC002", Barcode="8931234000208", CategoryId=sauces.Id, BaseUoMId=liter.Id, SellingPrice=0, MinStockLevel=10, IsPerishable=true }
            };

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProductConversionsAsync()
        {
            if (_context.ProductUoMConversions.Any()) return;

            var caseUom = await _context.UoMs.FirstAsync(x => x.Name == "Case");
            var bottle = await _context.UoMs.FirstAsync(x => x.Name == "Bottle");
            var gram = await _context.UoMs.FirstAsync(x => x.Name == "Gram");
            var kg = await _context.UoMs.FirstAsync(x => x.Name == "Kilogram");
            var ml = await _context.UoMs.FirstAsync(x => x.Name == "Milliliter");
            var liter = await _context.UoMs.FirstAsync(x => x.Name == "Liter");

            var beer = await _context.Products.FirstAsync(x => x.SKU == "BR001");
            var wine1 = await _context.Products.FirstAsync(x => x.SKU == "RW001");
            var wine2 = await _context.Products.FirstAsync(x => x.SKU == "RW002");
            var beef = await _context.Products.FirstAsync(x => x.SKU == "BF001");
            var milk = await _context.Products.FirstAsync(x => x.SKU == "DY001");

            var conversions = new List<ProductUoMConversion>
            {
                // Beer: 1 case = 12 bottle
                new ProductUoMConversion { ProductId=beer.Id, FromUoMId=caseUom.Id, ToUoMId=bottle.Id, Factor=12 },

                // Wine: 1 case = 6 bottle
                new ProductUoMConversion { ProductId=wine1.Id, FromUoMId=caseUom.Id, ToUoMId=bottle.Id, Factor=6 },
                new ProductUoMConversion { ProductId=wine2.Id, FromUoMId=caseUom.Id, ToUoMId=bottle.Id, Factor=6 },

                // Beef: 1 kg = 1000 g , 1 case = 10 kg
                new ProductUoMConversion { ProductId=beef.Id, FromUoMId=kg.Id, ToUoMId=gram.Id, Factor=1000 },
                new ProductUoMConversion { ProductId=beef.Id, FromUoMId=kg.Id, ToUoMId=caseUom.Id, Factor=10 },

                // Milk: 1 liter = 1000 ml
                new ProductUoMConversion { ProductId=milk.Id, FromUoMId=liter.Id, ToUoMId=ml.Id, Factor=1000 }
            };

            _context.ProductUoMConversions.AddRange(conversions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedSupplierProductPricesAsync()
        {
            if (_context.SupplierProductPrices.Any()) return;

            var random = new Random();

            var supplierProductPrices = new List<SupplierProductPrice>();

            for (int supplierId = 1; supplierId <= 10; supplierId++)
            {
                // mỗi supplier có giá cho 5–10 sản phẩm
                var productCount = random.Next(5, 11);

                var productIds = Enumerable.Range(1, 20)
                                           .OrderBy(x => random.Next())
                                           .Take(productCount)
                                           .ToList();

                foreach (var productId in productIds)
                {
                    var price = random.Next(50_000, 500_000);

                    supplierProductPrices.Add(
                        new SupplierProductPrice
                        {
                            ProductId = productId,
                            SupplierId = supplierId,
                            UnitPrice = price,
                            EffectiveDate = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                        }
                    );
                }
            }

            _context.SupplierProductPrices.AddRange(supplierProductPrices);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Flow Data

        public async Task SeedFlowDataAsync()
        {
            if (!await _context.PurchaseOrders.AnyAsync())
                await SeedPurchaseOrdersAsync();

            if (!await _context.GoodsReceipts.AnyAsync())
                await SeedGoodsReceiptsAsync();

            if (!await _context.SalesOrders.AnyAsync())
            {
                await SeedSalesOrdersAsync();
                await PostHalfSalesOrdersAsync();
            }

            if (!await _context.Deliveries.AnyAsync())
                await SeedDeliveriesAsync();

            if (!await _context.Invoices.AnyAsync())
                await SeedInvoicesAsync();
        }

        private async Task SeedPurchaseOrdersAsync()
        {
            if (_context.PurchaseOrders.Any()) return;

            var random = new Random();
            var purchaseOrders = new List<PurchaseOrder>();

            int sharedProductId = 1;

            for (int i = 1; i <= 10; i++)
            {
                var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30));

                var po = new PurchaseOrder
                {
                    OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{i:D3}",
                    SupplierId = random.Next(1, 6),
                    Status = PurchaseOrderStatus.Approved,
                    OrderDate = orderDate,
                    ApprovedDate = orderDate.AddDays(1),
                    Lines = new List<PurchaseOrderLine>()
                };

                int lineCount = random.Next(2, 5);

                var productIds = Enumerable.Range(1, 20)
                    .OrderBy(x => random.Next())
                    .Take(lineCount)
                    .ToList();

                // đảm bảo ProductId = 1 tồn tại
                if (!productIds.Contains(sharedProductId))
                    productIds[0] = sharedProductId;

                decimal totalAmount = 0;

                foreach (var productId in productIds)
                {
                    var orderedQty = random.Next(20, 100);
                    var unitPrice = random.Next(50_000, 500_000);

                    var line = new PurchaseOrderLine
                    {
                        ProductId = productId,
                        OrderedQty = orderedQty,
                        ReceivedQty = 0,
                        UnitPrice = unitPrice
                    };

                    totalAmount += orderedQty * unitPrice;
                    po.Lines.Add(line);
                }

                po.TotalAmount = totalAmount;
                purchaseOrders.Add(po);
            }

            _context.PurchaseOrders.AddRange(purchaseOrders);
            await _context.SaveChangesAsync();
        }

        private async Task SeedGoodsReceiptsAsync()
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(x => x.Lines)
                .Where(x => x.Status == PurchaseOrderStatus.Approved)
                .ToListAsync();

            if (!purchaseOrders.Any())
                return;

            var random = new Random();
            foreach (var po in purchaseOrders)
            {
                var linesDto = new List<CreateGoodsReceiptLineDto>();

                foreach (var line in po.Lines)
                {
                    var receivedQty = Math.Floor(
                        line.OrderedQty * (decimal)(0.6 + random.NextDouble() * 0.3));

                    if (receivedQty <= 0)
                        continue;

                    linesDto.Add(new CreateGoodsReceiptLineDto
                    {
                        PurchaseOrderId = po.Id,
                        ProductId = line.ProductId,
                        ReceivedQty = receivedQty
                    });
                }

                if (!linesDto.Any())
                    continue;

                var createDto = new CreateGoodsReceiptDto
                {
                    PurchaseOrderId = po.Id,
                    WarehouseId = random.Next(1, 3),
                    LinesDto = linesDto
                };

                var result = await _goodsReceiptService.CreateAsync(createDto);

                if (!result.IsSuccess)
                    continue;

                await _goodsReceiptService.PostAsync(result.Data.Id);
            }
        }

        private async Task SeedSalesOrdersAsync()
        {
            if (_context.SalesOrders.Any())
                return;

            var random = new Random();
            var salesOrders = new List<SalesOrder>();

            var productsWithLayers = await _context.InventoryCostLayers
                .GroupBy(x => x.ProductId)
                .Where(g => g.Count() >= 2)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Layers = g.OrderBy(x => x.ReceiptDate).ToList()
                })
                .ToListAsync();

            int soIndex = 1;

            // tạo nhiều SO split
            foreach (var p in productsWithLayers.Take(3)) // muốn nhiều hơn tăng số này
            {
                var layer1 = p.Layers[0];
                var layer2 = p.Layers[1];

                var available1 = layer1.RemainingQty - layer1.ReservedQty;
                var available2 = layer2.RemainingQty - layer2.ReservedQty;

                if (available1 <= 0 || available2 <= 0)
                    continue;

                var splitQty = Math.Min(available1 + available2 - 1, available1 + random.Next(1, 10));

                var so = new SalesOrder
                {
                    OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{soIndex++:D3}",
                    CustomerId = random.Next(1, 6),
                    OrderDate = DateTime.UtcNow,
                    Status = SalesOrderStatus.Draft,
                    Lines = new List<SalesOrderLine>
                    {
                        new SalesOrderLine
                        {
                            ProductId = p.ProductId,
                            RowNumber = 1,
                            OrderedQty = splitQty,
                            UnitCost = 0,
                            UnitPrice = 0
                        }
                    }
                };

                salesOrders.Add(so);
            }

            // thêm SO random nhiều line
            for (int i = 0; i < 5; i++)
            {
                var so = new SalesOrder
                {
                    OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{soIndex++:D3}",
                    CustomerId = random.Next(1, 6),
                    OrderDate = DateTime.UtcNow,
                    Status = SalesOrderStatus.Draft,
                    Lines = new List<SalesOrderLine>()
                };

                var lineCount = random.Next(2, 4);

                var productIds = await _context.Products
                    .Select(p => p.Id)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(lineCount)
                    .ToListAsync();

                int row = 1;

                foreach (var productId in productIds)
                {
                    var layers = await _context.InventoryCostLayers
                        .Where(x => x.ProductId == productId)
                        .ToListAsync();

                    if (!layers.Any())
                        continue;

                    var totalAvailable = layers.Sum(x => x.RemainingQty - x.ReservedQty);

                    if (totalAvailable <= 0)
                        continue;

                    var qty = random.Next(1, (int)Math.Min(totalAvailable, 20));

                    so.Lines.Add(new SalesOrderLine
                    {
                        ProductId = productId,
                        RowNumber = row++,
                        OrderedQty = qty,
                        UnitCost = 0,
                        UnitPrice = 0
                    });
                }

                if (so.Lines.Any())
                    salesOrders.Add(so);
            }

            _context.SalesOrders.AddRange(salesOrders);
            await _context.SaveChangesAsync();
        }

        private async Task PostHalfSalesOrdersAsync()
        {
            var salesOrders = await _context.SalesOrders
                .OrderBy(x => x.Id)
                .ToListAsync();

            var halfCount = salesOrders.Count / 2;

            var toPost = salesOrders
                .Take(halfCount)
                .Select(x => x.Id)
                .ToList();

            foreach (var soId in toPost)
            {
                await _salesOrderService.ConfirmAsync(soId);
            }
        }

        public async Task SeedDeliveriesAsync(CancellationToken cancellationToken = default)
        {
            if (await _context.Deliveries.AnyAsync())
                return;

            var salesOrders = await _salesOrderService.GetConfirmedSalesOrders(cancellationToken);
            if (!salesOrders.IsSuccess)
                throw new Exception(salesOrders.ErrorMessage);

            foreach (var so in salesOrders.Data)
            {
                var deliveryLines = so.Lines
                    .Where(x => x.RemainingQty > 0)
                    .Select(x => new CreateDeliveryLineDto
                    {
                        ProductId = x.ProductId,
                        RowNumber = x.RowNumber,
                        DeliveredQty = x.RemainingQty
                    })
                    .ToList();

                if (!deliveryLines.Any())
                    continue;

                var createDto = new CreateDeliveryDto
                {
                    SalesOrderId = so.Id,
                    LinesDto = deliveryLines
                };

                var createResult = await _deliveryService.CreateAsync(createDto, cancellationToken);

                if (!createResult.IsSuccess)
                    throw new Exception(createResult.ErrorMessage);

                var deliveryId = createResult.Data.Id;

                var postResult = await _deliveryService.PostAsync(deliveryId, cancellationToken);

                if (!postResult.IsSuccess)
                    throw new Exception(postResult.ErrorMessage);
            }
        }

        public async Task SeedInvoicesAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Invoices.Any())
                return;

            var deliveries = await _deliveryService.GetPostedDeliveriesWithLinesAsync(cancellationToken);
            if(!deliveries.IsSuccess)
                throw new Exception(deliveries.ErrorMessage);

            foreach (var delivery in deliveries.Data)
            {
                var createDto = new CreateInvoiceDto
                {
                    DeliveryId = delivery.Id,
                    CreateInvoiceLineDtos = delivery.Lines
                        .Where(l => l.RemainingInvoicedQty > 0)
                        .Select(l => new CreateInvoiceLineDto
                        {
                            ProductId = l.ProductId,
                            RowNumber = l.RowNumber,
                            InvoiceQuantity = (int)l.RemainingInvoicedQty
                        })
                        .ToList()
                };

                if (!createDto.CreateInvoiceLineDtos.Any())
                    continue;

                var createResult = await _invoiceService.CreateAsync(createDto, cancellationToken);

                if (!createResult.IsSuccess)
                    throw new Exception(createResult.ErrorMessage);

                var postResult = await _invoiceService.PostAsync(createResult.Data.Id, cancellationToken);

                if (!postResult.IsSuccess)
                    throw new Exception(postResult.ErrorMessage);
            }
        }

        #endregion

        #region Cache Data

        public async Task SeedProductSellingPriceCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if(_context.GoodsReceipts.Any() && _context.Products.Any() && _context.InventoryCostLayers.Any())
                {
                    var productIds = await _context.InventoryCostLayers
                        .Select(x => x.ProductId)
                        .Distinct()
                        .ToListAsync();

                    foreach ( var productId in productIds)
                        await _productService.SetProductSellingPrice(productId, cancellationToken);

                    // changes in db
                    await _context.SaveChangesAsync(cancellationToken);

                    var products_selling_price = await _context.Products
                        .Select(p => new { p.Id, p.Name, p.SellingPrice })
                        .ToListAsync();

                    foreach (var p in products_selling_price)
                    {
                        var cacheKey = CF.GetCachedProductSellingPriceKey(p.Id);
                        var cacheValue = new ProductSellingPrice
                        {
                            Id = p.Id,
                            Name = p.Name,
                            SellingPrice = p.SellingPrice
                        };
                        await _cacheService.SetAsync<ProductSellingPrice>(cacheKey, cacheValue, TimeSpan.FromHours(1));
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion
    }
}
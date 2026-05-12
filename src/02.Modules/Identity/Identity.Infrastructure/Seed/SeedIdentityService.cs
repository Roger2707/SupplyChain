using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Seed
{
    public class SeedIdentityService
    {
        private readonly IdentityDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public SeedIdentityService(IdentityDbContext context, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (!await _context.Users.AnyAsync())
                    await SeedUserAsyc();

                if (!await _context.Roles.AnyAsync())
                    await SeedRoleAsync();

                if (await _context.Users.AnyAsync() && await _context.Roles.AnyAsync() && !await _context.UserRoles.AnyAsync())
                    await SeedUserRoleAsync();

                if (await _context.Users.AnyAsync() && !await _context.UserWarehouses.AnyAsync())
                    await SeedUserWarehousesAsync();

                if (!await _context.Permissions.AnyAsync())
                    await SeedPermissionsAsync();

                if (await _context.Roles.AnyAsync() && await _context.Permissions.AnyAsync() && !await _context.RolePermissions.AnyAsync())
                    await SeedRolePermissionsAsync();

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork?.RollbackTransactionAsync();
                return;
            }
        }

        #region Functions

        private async Task SeedUserAsyc()
        {
            var users = new List<User>
            {
                new User
                {
                    Username = "rogersa",
                    FullName = "Roger SA",
                    Email = "rogersa@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "01 Le Duan, P.Ben Thanh, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Rogersa@123"),
                },
                new User
                {
                    Username = "greatorm",
                    FullName = "Greato RM",
                    Email = "greatorm@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "03 PDP, P.Cau Kieu, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Greatorm@123"),
                },
                new User
                {
                    Username = "baoanrm",
                    FullName = "Bao An RM",
                    Email = "baoanrm@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "05 Van Kiep, P.Phu Nhuan, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Baoanrm@123"),
                },
                new User
                {
                    Username = "gapuwm",
                    FullName = "Gapu Truong WM",
                    Email = "gapuwm@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "07 Tan Dinh, P.Tan Dinh, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Gapuwm@123"),
                },
                new User
                {
                    Username = "nguyenst",
                    FullName = "Truong Nguyen ST",
                    Email = "nguyenwm@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "09 Phan Dang Luu, P.Thanh My Tay, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Nguyenwm@123"),
                },
                new User
                {
                    Username = "duthst",
                    FullName = "Duc Thang ST",
                    Email = "duthwm@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "11 Quang Trung, P.Go Vap, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Duthwm@123"),
                },
                new User
                {
                    Username = "quincyst",
                    FullName = "Quincy ST",
                    Email = "quincyst@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "13 Pham Van Dong, P.Thu Duc, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Quincyst@123"),
                },
                new User
                {
                    Username = "alliest",
                    FullName = "Thuy Hang ST",
                    Email = "alliest@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "15 Ben Van Don, P.Pham Ngu Lao, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Alliest@123"),
                },
                new User
                {
                    Username = "mist",
                    FullName = "Nhu Quynh ST",
                    Email = "mist@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "17 Hoang Hoa Tham, P.Bien Hoa, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Mist@123"),
                },
            };

            foreach (var user in users)
                _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        private async Task SeedRoleAsync()
        {
            var roles = new List<Role>
            {
                new Role
                {
                    RoleName = "Super_Admin",
                    Description = "Full Access anywhere in application.",
                    RoleLevel = RoleLevel.SuperAdmin,
                },
                new Role
                {
                    RoleName = "Warehouse_Manager",
                    Description = "Full Access in warehouse they manage.",
                    RoleLevel = RoleLevel.WarehouseManager,
                },
                new Role
                {
                    RoleName = "Staff",
                    Description = "Limit access in warehouse they work.",
                    RoleLevel = RoleLevel.Staff,
                },
            };

            foreach (var role in roles)
                _context.Roles.Add(role);

            await _context.SaveChangesAsync();
        }

        private async Task SeedUserRoleAsync()
        {
            var users = _context.Users.ToDictionary(x => x.Username, x => x.Id);
            var roles = _context.Roles.ToDictionary(x => x.RoleName, x => x.Id);    
            var userRoles = new List<UserRole>
            {
                // ROLE SUPER_ADMIN
                new UserRole
                {
                    UserId = users["rogersa"],
                    RoleId = roles["Super_Admin"],
                },
                // ROLE WAREHOUSE_MANAGER
                new UserRole
                {
                    UserId =  users["greatorm"],
                    RoleId = roles["Warehouse_Manager"],
                },
                new UserRole
                {
                    UserId = users["baoanrm"],
                    RoleId = roles["Warehouse_Manager"],
                },
                new UserRole
                {
                    UserId =  users["gapuwm"],
                    RoleId = roles["Warehouse_Manager"],
                },
                // ROLE STAFF
                new UserRole
                {
                    UserId = users["nguyenst"],
                    RoleId = roles["Staff"],
                },
                new UserRole
                {
                    UserId = users["duthst"],
                    RoleId = roles["Staff"],
                },
                new UserRole
                {
                    UserId = users["quincyst"],
                    RoleId = roles["Staff"],
                },
                new UserRole
                {
                    UserId = users["alliest"],
                    RoleId = roles["Staff"],
                },
                new UserRole
                {
                    UserId = users["mist"],
                    RoleId = roles["Staff"],
                },
            };

            foreach (var ur in userRoles)
                _context.UserRoles.Add(ur);

            await _context.SaveChangesAsync();
        }

        private async Task SeedUserWarehousesAsync()
        {
            var users = _context.Users.ToDictionary(x => x.Username, x => x.Id);
            var userWarehouses = new List<UserWarehouse>
            {
                // Warehouse Manager is in their own warehouse
                new UserWarehouse { UserId = users["greatorm"], WarehouseId = 1 },
                new UserWarehouse { UserId = users["baoanrm"], WarehouseId = 2 },
                new UserWarehouse { UserId = users["gapuwm"], WarehouseId = 2 },

                // Staff in each warehouse
                new UserWarehouse { UserId = users["quincyst"], WarehouseId = 1 },
                new UserWarehouse { UserId = users["alliest"], WarehouseId = 1 },

                new UserWarehouse { UserId = users["mist"], WarehouseId = 2 },

                new UserWarehouse { UserId = users["duthst"], WarehouseId = 3 },
                new UserWarehouse { UserId = users["nguyenst"], WarehouseId = 3 },
            };

            foreach (var uw in userWarehouses)
                _context.UserWarehouses.Add(uw);

            await _context.SaveChangesAsync();
        }

        private async Task SeedPermissionsAsync()
        {
            var permissions = new List<Permission>
            {
                #region Warehouse Permissions

                new Permission
                {
                    PermissionName = "WAREHOUSE_CREATE",
                    Module = "Warehouse",
                    Action = "Create",
                    Description = "Create new warehouse",
                },
                new Permission
                {
                    PermissionName = "WAREHOUSE_EDIT",
                    Module = "Warehouse",
                    Action = "Update",
                    Description = "Update warehouse information .",
                },
                new Permission
                {
                    PermissionName = "WAREHOUSE_DELETE",
                    Module = "Warehouse",
                    Action = "Delete",
                    Description = "Soft delete warehouse .",
                },
                new Permission
                {
                    PermissionName = "WAREHOUSE_VIEW",
                    Module = "Warehouse",
                    Action = "View",
                    Description = "View warehouse",
                },

                #endregion

                #region PurchaseOrder Permissions

                new Permission
                {
                    PermissionName = "PURCHASEORDER_CREATE",
                    Module = "PurchaseOrder",
                    Action = "Create",
                    Description = "Create PurchaseOrder. ",              
                },               
                new Permission
                {
                    PermissionName = "PURCHASEORDER_EDIT",
                    Module = "PurchaseOrder",
                    Action = "Edit",
                    Description = "Edit PurchaseOrder. ",                   
                },
                new Permission
                {
                    PermissionName = "PURCHASEORDER_DELETE",
                    Module = "PurchaseOrder",
                    Action = "Delete",
                    Description = "Delete PurchaseOrder. ",                  
                },
                new Permission
                {
                    PermissionName = "PURCHASEORDER_VIEW",
                    Module = "PurchaseOrder",
                    Action = "View",
                    Description = "View PurchaseOrder. ",                   
                },

                new Permission
                {
                    PermissionName = "PURCHASEORDER_POST",
                    Module = "PurchaseOrder",
                    Action = "Post",
                    Description = "Post PurchaseOrder. ",                   
                },

                new Permission
                {
                    PermissionName = "PURCHASEORDER_CANCEL",
                    Module = "PurchaseOrder",
                    Action = "Cancel",
                    Description = "Cancel PurchaseOrder. ",                  
                },

                #endregion

                #region GoodsReceipt Permissions

                new Permission
                {
                    PermissionName = "GOODSRECEIPT_CREATE",
                    Module = "GOODSRECEIPT",
                    Action = "Create",
                    Description = "Create GOODSRECEIPT. ",                 
                },
                new Permission
                {
                    PermissionName = "GOODSRECEIPT_EDIT",
                    Module = "GOODSRECEIPT",
                    Action = "Edit",
                    Description = "Edit GOODSRECEIPT. ",                    
                },
                new Permission
                {
                    PermissionName = "GOODSRECEIPT_DELETE",
                    Module = "GOODSRECEIPT",
                    Action = "Delete",
                    Description = "Delete GOODSRECEIPT. ",                    
                },
                new Permission
                {
                    PermissionName = "GOODSRECEIPT_VIEW",
                    Module = "GOODSRECEIPT",
                    Action = "View",
                    Description = "View GOODSRECEIPT. ",                   
                },

                new Permission
                {
                    PermissionName = "GOODSRECEIPT_POST",
                    Module = "GOODSRECEIPT",
                    Action = "Post",
                    Description = "Post GOODSRECEIPT. ",                   
                },

                new Permission
                {
                    PermissionName = "GOODSRECEIPT_CANCEL",
                    Module = "GOODSRECEIPT",
                    Action = "Cancel",
                    Description = "Cancel GOODSRECEIPT. ",                   
                },

                #endregion

                #region SalesOrder Permissions

                new Permission
                {
                    PermissionName = "SALESORDER_CREATE",
                    Module = "SALESORDER",
                    Action = "Create",
                    Description = "Create SALESORDER. ",                 
                },
                new Permission
                {
                    PermissionName = "SALESORDER_EDIT",
                    Module = "SALESORDER",
                    Action = "Edit",
                    Description = "Edit SALESORDER. ",                    
                },
                new Permission
                {
                    PermissionName = "SALESORDER_DELETE",
                    Module = "SALESORDER",
                    Action = "Delete",
                    Description = "Delete SALESORDER. ",                  
                },
                new Permission
                {
                    PermissionName = "SALESORDER_VIEW",
                    Module = "SALESORDER",
                    Action = "View",
                    Description = "View SALESORDER. ",                   
                },

                new Permission
                {
                    PermissionName = "SALESORDER_POST",
                    Module = "SALESORDER",
                    Action = "Post",
                    Description = "Post SALESORDER. ",                  
                },

                new Permission
                {
                    PermissionName = "SALESORDER_CANCEL",
                    Module = "SALESORDER",
                    Action = "Cancel",
                    Description = "Cancel SALESORDER. ",                
                },

                #endregion

                #region DELIVERY Permissions

                new Permission
                {
                    PermissionName = "DELIVERY_CREATE",
                    Module = "DELIVERY",
                    Action = "Create",
                    Description = "Create DELIVERY. ",                   
                },
                new Permission
                {
                    PermissionName = "DELIVERY_EDIT",
                    Module = "DELIVERY",
                    Action = "Edit",
                    Description = "Edit DELIVERY. ",                  
                },
                new Permission
                {
                    PermissionName = "DELIVERY_DELETE",
                    Module = "DELIVERY",
                    Action = "Delete",
                    Description = "Delete DELIVERY. ",                 
                },
                new Permission
                {
                    PermissionName = "DELIVERY_VIEW",
                    Module = "DELIVERY",
                    Action = "View",
                    Description = "View DELIVERY. ",             
                },
                new Permission
                {
                    PermissionName = "DELIVERY_POST",
                    Module = "DELIVERY",
                    Action = "Post",
                    Description = "Post DELIVERY. ",                 
                },
                new Permission
                {
                    PermissionName = "DELIVERY_CANCEL",
                    Module = "DELIVERY",
                    Action = "Cancel",
                    Description = "Cancel DELIVERY. ",                   
                },

                #endregion

                #region INVOICE Permissions

                new Permission
                {
                    PermissionName = "INVOICE_CREATE",
                    Module = "INVOICE",
                    Action = "Create",
                    Description = "Create INVOICE. ",            
                },
                new Permission
                {
                    PermissionName = "INVOICE_EDIT",
                    Module = "INVOICE",
                    Action = "Edit",
                    Description = "Edit INVOICE. ",                   
                },
                new Permission
                {
                    PermissionName = "INVOICE_DELETE",
                    Module = "INVOICE",
                    Action = "Delete",
                    Description = "Delete INVOICE. ",                 
                },
                new Permission
                {
                    PermissionName = "INVOICE_VIEW",
                    Module = "INVOICE",
                    Action = "View",
                    Description = "View INVOICE. ",                   
                },

                new Permission
                {
                    PermissionName = "INVOICE_POST",
                    Module = "INVOICE",
                    Action = "Post",
                    Description = "Post INVOICE. ",                   
                },

                new Permission
                {
                    PermissionName = "INVOICE_CANCEL",
                    Module = "INVOICE",
                    Action = "Cancel",
                    Description = "Cancel INVOICE. ",                    
                },

                #endregion

                #region Product Permmisions

                new Permission
                {
                    PermissionName = "PRODUCT_VIEW",
                    Module = "Product",
                    Action = "View",
                    Description = "View Product. ",                  
                },
                new Permission
                {
                    PermissionName = "PRODUCT_CREATE",
                    Module = "Product",
                    Action = "Create",
                    Description = "Create new product in application .",
                },
                new Permission
                {
                    PermissionName = "PRODUCT_EDIT",
                    Module = "Product",
                    Action = "Edit",
                    Description = "Edit product in application .",
                },
                new Permission
                {
                    PermissionName = "PRODUCT_DELETE",
                    Module = "Product",
                    Action = "Delete",
                    Description = "Delete product in application .",
                },

                #endregion

            };

            foreach (var permission in permissions)
                _context.Permissions.Add(permission);

            await _context.SaveChangesAsync();
        }

        private async Task SeedRolePermissionsAsync()
        {
            var roles = _context.Roles.ToDictionary(x => x.RoleName, x => x.Id);
            var permissions = _context.Permissions.ToDictionary(x => x.PermissionName, x => x.Id);

            var rolePermissions = new List<RolePermission>
            {
                // Warehouse_Magager 's Permissions
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["WAREHOUSE_UPDATE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["WAREHOUSE_DELETE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["WAREHOUSE_VIEW"]},

                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PURCHASEORDER_CREATE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PURCHASEORDER_DELETE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PURCHASEORDER_EDIT"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PURCHASEORDER_VIEW"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PURCHASEORDER_POST"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PURCHASEORDER_CANCEL"]},

                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["GOODSRECEIPT_CREATE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["GOODSRECEIPT_DELETE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["GOODSRECEIPT_EDIT"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["GOODSRECEIPT_VIEW"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["GOODSRECEIPT_POST"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["GOODSRECEIPT_CANCEL"]},
                                                                                                     
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["SALESORDER_CREATE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["SALESORDER_DELETE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["SALESORDER_EDIT"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["SALESORDER_VIEW"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["SALESORDER_POST"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["SALESORDER_CANCEL"]},
                                                                                                     
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["DELIVERY_CREATE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["DELIVERY_DELETE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["DELIVERY_EDIT"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["DELIVERY_VIEW"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["DELIVERY_POST"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["DELIVERY_CANCEL"]},

                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["INVOICE_CREATE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["INVOICE_DELETE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["INVOICE_EDIT"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["INVOICE_VIEW"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["INVOICE_POST"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["INVOICE_CANCEL"]},

                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PRODUCT_CREATE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PRODUCT_DELETE"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PRODUCT_EDIT"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PRODUCT_VIEW"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PRODUCT_POST"]},
                new RolePermission { RoleId = roles["Warehouse_Manager"], PermissionId = permissions["PRODUCT_CANCEL"]},


                 // Staff 's Permissions
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["WAREHOUSE_VIEW"]},

                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["PURCHASEORDER_CREATE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["PURCHASEORDER_EDIT"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["PURCHASEORDER_DELETE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["PURCHASEORDER_VIEW"]},

                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["GOODSRECEIPT_CREATE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["GOODSRECEIPT_EDIT"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["GOODSRECEIPT_DELETE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["GOODSRECEIPT_VIEW"]},

                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["SALESORDER_CREATE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["SALESORDER_EDIT"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["SALESORDER_DELETE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["SALESORDER_VIEW"]},

                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["DELIVERY_CREATE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["DELIVERY_EDIT"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["DELIVERY_DELETE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["DELIVERY_VIEW"]},

                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["INVOICE_CREATE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["INVOICE_EDIT"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["INVOICE_DELETE"]},
                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["INVOICE_VIEW"]},

                new RolePermission { RoleId = roles["Staff"], PermissionId = permissions["PRODUCT_VIEW"]},
            };

            foreach (var rp in rolePermissions)
                _context.RolePermissions.Add(rp);

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}

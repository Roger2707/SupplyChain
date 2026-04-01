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
                    Username = "nguyenwm",
                    FullName = "Truong Nguyen WM",
                    Email = "nguyenwm@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "09 Phan Dang Luu, P.Thanh My Tay, HCMC",
                    PasswordHash = _passwordHasher.HashPassword("Nguyenwm@123"),
                },
                new User
                {
                    Username = "duthwm",
                    FullName = "Duc Thang WM",
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
            var user_ids = _context.Users.Select(u => u.Id).ToList();
            var role_ids = _context.Roles.Select(r => r.Id).ToList();
            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = user_ids[0],
                    RoleId = role_ids[0],
                },
                new UserRole
                {
                    UserId =  user_ids[1],
                    RoleId = role_ids[1],
                },
                new UserRole
                {
                    UserId = user_ids[2],
                    RoleId = role_ids[1],
                },
                new UserRole
                {
                    UserId = user_ids[3],
                    RoleId = role_ids[2],
                },
                new UserRole
                {
                    UserId = user_ids[4],
                    RoleId = role_ids[2],
                },
                new UserRole
                {
                    UserId = user_ids[5],
                    RoleId = role_ids[2],
                },
                new UserRole
                {
                    UserId = user_ids[6],
                    RoleId = role_ids[2],
                },
                new UserRole
                {
                    UserId = user_ids[7],
                    RoleId = role_ids[2],
                },
                new UserRole
                {
                    UserId = user_ids[8],
                    RoleId = role_ids[2],
                },
            };

            foreach (var ur in userRoles)
                _context.UserRoles.Add(ur);

            await _context.SaveChangesAsync();
        }

        private async Task SeedUserWarehousesAsync()
        {
            var user_ids = _context.Users.Select(u => u.Id).ToList();
            var userWarehouses = new List<UserWarehouse>
            {
                // Warehouse Manager is in their own warehouse
                new UserWarehouse { UserId = user_ids[1], WarehouseId = 1, IsWarehouseManager = true },
                new UserWarehouse { UserId = user_ids[2], WarehouseId = 2, IsWarehouseManager = true },

                // Staff in each warehouse
                new UserWarehouse { UserId = user_ids[3], WarehouseId = 1 },
                new UserWarehouse { UserId = user_ids[4], WarehouseId = 1 },
                new UserWarehouse { UserId = user_ids[5], WarehouseId = 1 },

                new UserWarehouse { UserId = user_ids[6], WarehouseId = 2 },
                new UserWarehouse { UserId = user_ids[7], WarehouseId = 2 },
                new UserWarehouse { UserId = user_ids[8], WarehouseId = 2 },
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
                    PermissionScope = PermissionScope.System
                },
                new Permission
                {
                    PermissionName = "WAREHOUSE_UPDATE",
                    Module = "Warehouse",
                    Action = "Update",
                    Description = "Update warehouse information .",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "WAREHOUSE_DELETE",
                    Module = "Warehouse",
                    Action = "Delete",
                    Description = "Soft delete warehouse .",
                    PermissionScope = PermissionScope.System
                },
                new Permission
                {
                    PermissionName = "WAREHOUSE_VIEW",
                    Module = "Warehouse",
                    Action = "View",
                    Description = "View warehouse",
                    PermissionScope = PermissionScope.Warehouse
                },

                #endregion

                #region PurchaseOrder Permissions

                new Permission
                {
                    PermissionName = "PURCHASEORDER_CREATE",
                    Module = "PurchaseOrder",
                    Action = "Create",
                    Description = "Create PurchaseOrder. ",
                    PermissionScope = PermissionScope.Warehouse
                },               
                new Permission
                {
                    PermissionName = "PURCHASEORDER_EDIT",
                    Module = "PurchaseOrder",
                    Action = "Edit",
                    Description = "Edit PurchaseOrder. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "PURCHASEORDER_DELETE",
                    Module = "PurchaseOrder",
                    Action = "Delete",
                    Description = "Delete PurchaseOrder. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "PURCHASEORDER_VIEW",
                    Module = "PurchaseOrder",
                    Action = "View",
                    Description = "View PurchaseOrder. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "PURCHASEORDER_POST",
                    Module = "PurchaseOrder",
                    Action = "Post",
                    Description = "Post PurchaseOrder. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "PURCHASEORDER_CANCEL",
                    Module = "PurchaseOrder",
                    Action = "Cancel",
                    Description = "Cancel PurchaseOrder. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                #endregion

                #region GoodsReceipt Permissions

                new Permission
                {
                    PermissionName = "GOODSRECEIPT_CREATE",
                    Module = "GOODSRECEIPT",
                    Action = "Create",
                    Description = "Create GOODSRECEIPT. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "GOODSRECEIPT_EDIT",
                    Module = "GOODSRECEIPT",
                    Action = "Edit",
                    Description = "Edit GOODSRECEIPT. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "GOODSRECEIPT_DELETE",
                    Module = "GOODSRECEIPT",
                    Action = "Delete",
                    Description = "Delete GOODSRECEIPT. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "GOODSRECEIPT_VIEW",
                    Module = "GOODSRECEIPT",
                    Action = "View",
                    Description = "View GOODSRECEIPT. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "GOODSRECEIPT_POST",
                    Module = "GOODSRECEIPT",
                    Action = "Post",
                    Description = "Post GOODSRECEIPT. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "GOODSRECEIPT_CANCEL",
                    Module = "GOODSRECEIPT",
                    Action = "Cancel",
                    Description = "Cancel GOODSRECEIPT. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                #endregion

                #region SalesOrder Permissions

                new Permission
                {
                    PermissionName = "SALESORDER_CREATE",
                    Module = "SALESORDER",
                    Action = "Create",
                    Description = "Create SALESORDER. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "SALESORDER_EDIT",
                    Module = "SALESORDER",
                    Action = "Edit",
                    Description = "Edit SALESORDER. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "SALESORDER_DELETE",
                    Module = "SALESORDER",
                    Action = "Delete",
                    Description = "Delete SALESORDER. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "SALESORDER_VIEW",
                    Module = "SALESORDER",
                    Action = "View",
                    Description = "View SALESORDER. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "SALESORDER_POST",
                    Module = "SALESORDER",
                    Action = "Post",
                    Description = "Post SALESORDER. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "SALESORDER_CANCEL",
                    Module = "SALESORDER",
                    Action = "Cancel",
                    Description = "Cancel SALESORDER. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                #endregion

                #region DELIVERY Permissions

                new Permission
                {
                    PermissionName = "DELIVERY_CREATE",
                    Module = "DELIVERY",
                    Action = "Create",
                    Description = "Create DELIVERY. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "DELIVERY_EDIT",
                    Module = "DELIVERY",
                    Action = "Edit",
                    Description = "Edit DELIVERY. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "DELIVERY_DELETE",
                    Module = "DELIVERY",
                    Action = "Delete",
                    Description = "Delete DELIVERY. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "DELIVERY_VIEW",
                    Module = "DELIVERY",
                    Action = "View",
                    Description = "View DELIVERY. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "DELIVERY_POST",
                    Module = "DELIVERY",
                    Action = "Post",
                    Description = "Post DELIVERY. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "DELIVERY_CANCEL",
                    Module = "DELIVERY",
                    Action = "Cancel",
                    Description = "Cancel DELIVERY. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                #endregion

                #region INVOICE Permissions

                new Permission
                {
                    PermissionName = "INVOICE_CREATE",
                    Module = "INVOICE",
                    Action = "Create",
                    Description = "Create INVOICE. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "INVOICE_EDIT",
                    Module = "INVOICE",
                    Action = "Edit",
                    Description = "Edit INVOICE. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "INVOICE_DELETE",
                    Module = "INVOICE",
                    Action = "Delete",
                    Description = "Delete INVOICE. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "INVOICE_VIEW",
                    Module = "INVOICE",
                    Action = "View",
                    Description = "View INVOICE. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "INVOICE_POST",
                    Module = "INVOICE",
                    Action = "Post",
                    Description = "Post INVOICE. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                new Permission
                {
                    PermissionName = "INVOICE_CANCEL",
                    Module = "INVOICE",
                    Action = "Cancel",
                    Description = "Cancel INVOICE. ",
                    PermissionScope = PermissionScope.Warehouse
                },

                #endregion

                #region Product Permmisions

                new Permission
                {
                    PermissionName = "PRODUCT_VIEW",
                    Module = "Product",
                    Action = "View",
                    Description = "View Product. ",
                    PermissionScope = PermissionScope.Warehouse
                },
                new Permission
                {
                    PermissionName = "PRODUCT_CREATE",
                    Module = "Product",
                    Action = "Create",
                    Description = "Create new product in application .",
                    PermissionScope = PermissionScope.System
                },
                new Permission
                {
                    PermissionName = "Product AccessPRODUCT_UPDATE_DELETE",
                    Module = "Product",
                    Action = "UpDel",
                    Description = "Update or Delete (soft delete) product in application .",
                    PermissionScope = PermissionScope.System
                },

                #endregion

            };

            foreach (var permission in permissions)
                _context.Permissions.Add(permission);

            await _context.SaveChangesAsync();
        }

        private async Task SeedRolePermissionsAsync()
        {
            var role_ids = _context.Roles.Select(x => x.Id).ToList();
            var permissions = _context.Permissions.Select(p => new { p.Id, p.PermissionName }).ToList();

            var rolePermissions = new List<RolePermission>
            {
                // Warehouse_Magager 's Permissions
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "WAREHOUSE_UPDATE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "WAREHOUSE_DELETE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "WAREHOUSE_VIEW").Id },

                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_CREATE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_DELETE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_EDIT").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_VIEW").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_POST").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_CANCEL").Id },
                
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_CREATE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_DELETE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_EDIT").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_VIEW").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_POST").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_CANCEL").Id },
                
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_CREATE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_DELETE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_EDIT").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_VIEW").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_POST").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_CANCEL").Id },
                
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_CREATE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_DELETE").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_EDIT").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_VIEW").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_POST").Id },
                new RolePermission { RoleId = role_ids[1], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_CANCEL").Id },


                 // Staff 's Permissions
                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "WAREHOUSE_VIEW").Id },

                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_CREATE").Id },
                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "PURCHASEORDER_VIEW").Id },

                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_CREATE").Id },
                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "GOODSRECEIPT_VIEW").Id },

                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_CREATE").Id },
                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "SALESORDER_VIEW").Id },

                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_CREATE").Id },
                new RolePermission { RoleId = role_ids[2], PermissionId = permissions.FirstOrDefault(p => p.PermissionName == "DELIVERY_VIEW").Id },
            };

            foreach (var rp in rolePermissions)
                _context.RolePermissions.Add(rp);

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}

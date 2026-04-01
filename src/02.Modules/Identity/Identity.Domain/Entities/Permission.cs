using SharedKernel.Entities;

namespace Identity.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public string PermissionName { get; set; }
        public string Module { get; set; } // Products, Warehouses
        public string Action { get; set; } // View, Create, Update, Delete, Approve, Import, Export
        public string Description { get; set; }
        public required PermissionScope PermissionScope { get; set; }

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public enum PermissionScope
    {
        System = 1,
        Region = 2,
        Warehouse = 3,
    }
}

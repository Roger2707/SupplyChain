namespace Inventory.Domain.Enums
{
    public enum SalesOrderStatus
    {
        Draft,              // mới tạo, sửa thoải mái
        Confirmed,          // đã xác nhận, đã reserve stock
        PartiallyDelivered, // đã giao một phần
        Delivered,          // giao hết
        Completed,          // giao + invoice + payment xong (optional)
        Cancelled
    }
}

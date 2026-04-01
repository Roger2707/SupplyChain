namespace Inventory.Domain.Enums
{
    public enum InventoryTransactionType
    {
        Receipt = 1,        // Nhập kho từ GoodsReceipt
        Issue = 2,          // Xuất kho (bán hàng / sử dụng)
        AdjustmentIn = 3,   // Điều chỉnh tăng kho
        AdjustmentOut = 4,  // Điều chỉnh giảm kho
        TransferIn = 5,     // Nhập kho do chuyển kho
        TransferOut = 6     // Xuất kho do chuyển kho
    }
}

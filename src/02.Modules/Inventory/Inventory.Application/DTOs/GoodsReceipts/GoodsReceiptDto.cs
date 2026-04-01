using Inventory.Domain.Enums;

namespace Inventory.Application.DTOs.GoodsReceipts;

public class GoodsReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public int WarehouseId { get; set; }
    public ReceiptStatus Status { get; set; }
    public DateTime ReceiptDate { get; set; }

    public byte[]? RowVersion { get; set; }

    public List<GoodsReceiptLineDto> Lines { get; set; } = new();
}


using Inventory.Domain.Enums;
using SharedKernel.Entities;

namespace Inventory.Domain.Entities.GoodsReceipt
{
    public class GoodsReceipt : BaseEntity
    {
        public string ReceiptNumber { get; set; }

        public int PurchaseOrderId { get; set; }

        public int WarehouseId { get; set; }

        public ReceiptStatus Status { get; set; }

        public DateTime ReceiptDate { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<GoodsReceiptLine> Lines { get; set; }

        public void Update(int warehouseId, DateTime receiptDate, List<(int productId, decimal poOrderedQty, decimal receivedQty, decimal unitCost)> updatedLines)
        {
            if (Status != ReceiptStatus.Draft)
                throw new Exception("Only Draft Status can be updated !");

            if (updatedLines == null || !updatedLines.Any())
                throw new Exception("There is not any details for this header !");

            WarehouseId = warehouseId;
            ReceiptDate = receiptDate;

            var linesToRemove = Lines.Where(l => !updatedLines.Any(ul => ul.productId == l.ProductId)).ToList();
            foreach (var line in linesToRemove)
                Lines.Remove(line);

            foreach (var updateLine in updatedLines)
            {
                if (updateLine.receivedQty <= 0)
                    throw new Exception("Received quantity must be > 0");

                if (updateLine.receivedQty > updateLine.poOrderedQty)
                    throw new Exception($"Product {updateLine.productId} in Purchase {PurchaseOrderId} can not be Receive {updateLine.receivedQty}, Ordered: {updateLine.poOrderedQty}");

                var existLine = Lines.FirstOrDefault(l => l.ProductId == updateLine.productId);
                if (existLine != null)
                {
                    existLine.ReceivedQty = updateLine.receivedQty;
                    existLine.UnitCost = updateLine.unitCost;
                }
                else
                {
                    var newLine = new GoodsReceiptLine
                    {
                        PurchaseOrderId = PurchaseOrderId,
                        ProductId = updateLine.productId,
                        ReceivedQty = updateLine.receivedQty,
                        UnitCost = updateLine.unitCost
                    };
                    Lines.Add(newLine);
                }
            }

            TotalAmount = Lines.Sum(l => l.LineTotal);
        }
    
        public void Post()
        {
            if(Status != ReceiptStatus.Draft)
                throw new Exception("Only Draft can post");

            Status = ReceiptStatus.Posted;
        }
    }
}


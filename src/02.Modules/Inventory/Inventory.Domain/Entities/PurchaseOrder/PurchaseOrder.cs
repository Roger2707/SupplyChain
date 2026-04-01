using Inventory.Domain.Enums;
using SharedKernel.Entities;

namespace Inventory.Domain.Entities.PurchaseOrder
{
    public class PurchaseOrder : BaseEntity
    {
        public string OrderNumber { get; set; }

        public int SupplierId { get; set; }

        public PurchaseOrderStatus Status { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<PurchaseOrderLine> Lines { get; set; }

        public void Approve()
        {
            if (Status != PurchaseOrderStatus.Draft)
                throw new Exception("Only Draft Status can be approved !");

            if (Lines == null || !Lines.Any())
                throw new Exception("There is not any details for this header !");

            Status = PurchaseOrderStatus.Approved;
            ApprovedDate = DateTime.UtcNow;
        }

        public void Update(int supplierId, DateTime orderDate, List<(int ProductId, decimal OrderQty, decimal UnitPrice)> updatedLines)
        {
            if (Status != PurchaseOrderStatus.Draft)
                throw new Exception("Only Draft Status can be update !");

            if (updatedLines == null || !updatedLines.Any())
                throw new Exception("There is not any details for this header !");

            // Update PO
            SupplierId = supplierId;
            OrderDate = orderDate;

            #region Update PO Lines

            // 1. Remove lines that are not in the updatedLines list
            var removedLine = Lines.Where(l => !updatedLines.Any(ln => ln.ProductId == l.ProductId)).ToList();
            
            foreach(var removeLine in removedLine)
                Lines.Remove(removeLine);

            // 2. Update existing lines and add new lines (if not exist)
            foreach (var updateLine in updatedLines)
            {
                var existLine = Lines.FirstOrDefault(l => l.ProductId == updateLine.ProductId);
                if(existLine != null)
                {
                    existLine.ProductId = updateLine.ProductId;
                    existLine.OrderedQty = updateLine.OrderQty;
                    existLine.UnitPrice = updateLine.UnitPrice;
                }
                else
                {
                    if (updateLine.OrderQty <= 0)
                        throw new Exception("Quantity must be > 0");

                    var newLine = new PurchaseOrderLine
                    {
                        PurchaseOrderId = Id,
                        ProductId = updateLine.ProductId,
                        OrderedQty = updateLine.OrderQty,
                        UnitPrice = updateLine.UnitPrice,
                    };

                    Lines.Add(newLine);
                }
            }

            // 3. Re-calculate Total Amount
            TotalAmount = Lines.Sum(l => l.LineTotal);

            #endregion
        }
    
        public void Delete()
        {
            if(Status == PurchaseOrderStatus.Draft)
            {
                IsDeleted = true;
            }
        }
    }
}

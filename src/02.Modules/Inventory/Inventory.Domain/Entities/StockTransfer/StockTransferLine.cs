using SharedKernel.Entities;

namespace Inventory.Domain.Entities.StockTransfer
{
    public class StockTransferLine : BaseEntity
    {
        public int StockTransferId { get; private set; }
        public int ProductId { get; private set; }
        public decimal Quantity { get; private set; }
    }
}

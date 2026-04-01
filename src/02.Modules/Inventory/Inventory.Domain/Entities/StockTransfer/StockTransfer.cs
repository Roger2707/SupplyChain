using Inventory.Domain.Enums;
using SharedKernel.Entities;

namespace Inventory.Domain.Entities.StockTransfer
{
    public class StockTransfer : BaseEntity
    {
        public string TransferNumber { get; private set; }

        public int FromWarehouseId { get; private set; }
        public int ToWarehouseId { get; private set; }

        public TransferStatus Status { get; private set; }

        public ICollection<StockTransferLine> Lines { get; private set; }
    }
}

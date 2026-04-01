using Inventory.Domain.Enums;
using SharedKernel.Entities;

namespace Inventory.Domain.Entities.SalesOrder
{
    public class SalesOrder : BaseEntity
    {
        public string OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime OrderDate { get; set; }
        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
        public decimal TotalAmount { get; set; }
        public List<SalesOrderLine> Lines { get; set; } = new();

        public bool AllowUpdate()
        {
            if(Status != SalesOrderStatus.Draft)
                return false;
            return true;
        }

        public void Confirm()
        {
            if (Status != SalesOrderStatus.Draft)
                throw new Exception("Only Draft Status can be Confirm");

            if(!Lines.Any() || Lines.Count == 0)
                throw new Exception("Lines have to have more than 0 items");

            Status = SalesOrderStatus.Confirmed;
        }
    }
}

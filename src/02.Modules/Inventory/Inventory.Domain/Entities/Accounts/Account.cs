using Inventory.Domain.Enums;
using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Accounts
{
    public class Account : BaseEntity
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public AccountType Type { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
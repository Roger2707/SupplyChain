namespace Inventory.Application.Interfaces.Generators
{
    public interface IInvoiceGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken);
    }
}

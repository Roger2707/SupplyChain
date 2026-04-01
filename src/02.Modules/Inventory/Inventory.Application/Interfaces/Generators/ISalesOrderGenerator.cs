namespace Inventory.Application.Interfaces.Generators
{
    public interface ISalesOrderGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken);
    }
}

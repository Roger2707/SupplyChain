namespace Inventory.Application.Interfaces.Generators
{
    public interface ISkuGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken);
    }
}

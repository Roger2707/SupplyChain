namespace Inventory.Application.Interfaces.Generators
{
    public interface IDeliveryGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken);
    }
}

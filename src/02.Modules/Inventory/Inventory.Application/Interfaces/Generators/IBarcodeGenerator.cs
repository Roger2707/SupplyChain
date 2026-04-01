namespace Inventory.Application.Interfaces.Generators
{
    public interface IBarcodeGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken);
    }
}

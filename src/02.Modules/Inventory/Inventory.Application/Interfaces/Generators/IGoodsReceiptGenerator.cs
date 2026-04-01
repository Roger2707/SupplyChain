namespace Inventory.Application.Interfaces.Generators
{
    public interface IGoodsReceiptGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken);
    }
}

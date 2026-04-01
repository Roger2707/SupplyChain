using Inventory.Application.Interfaces.Generators;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory.Infrastructure.Repositories.Generators
{
    public class DeliveryGenerator : IDeliveryGenerator
    {
        private readonly ApplicationDbContext _context;

        public DeliveryGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync(CancellationToken cancellationToken)
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR DeliverySequence";

            var transaction = _context.Database.CurrentTransaction;
            if (transaction != null)
                command.Transaction = transaction.GetDbTransaction();

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var nextValue = Convert.ToInt32(result);

            return $"DE-{DateTime.UtcNow:yyyyMMdd}-{nextValue:D3}";
        }
    }
}

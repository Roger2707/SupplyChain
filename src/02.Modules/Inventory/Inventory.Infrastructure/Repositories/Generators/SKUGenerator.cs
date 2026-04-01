using Inventory.Application.Interfaces.Generators;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Inventory.Infrastructure.Repositories.Generators
{
    public class SKUGenerator : ISkuGenerator
    {
        private readonly ApplicationDbContext _context;

        public SKUGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync(CancellationToken cancellationToken)
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR ProductSkuSequence";

            var result = await command.ExecuteScalarAsync(cancellationToken);

            var nextValue = Convert.ToInt32(result);

            return $"PRD-{nextValue:D6}";
        }
    }
}

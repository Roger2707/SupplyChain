using Inventory.Application.Interfaces.Generators;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories.Generators
{
    public class BarcodeGenerator : IBarcodeGenerator
    {
        private readonly ApplicationDbContext _context;

        private const string CountryPrefix = "893"; // VietNam's GS1 prefix
        private const string CompanyCode = "1234";
        public BarcodeGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync(CancellationToken cancellationToken)
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR ProductBarcodeSequence";

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var nextValue = Convert.ToInt32(result);

            var productPart = nextValue.ToString("D5"); // 5 digits

            var baseCode = $"{CountryPrefix}{CompanyCode}{productPart}"; // 12 digits

            var checksum = CalculateChecksum(baseCode);

            return $"{baseCode}{checksum}";
        }

        private static int CalculateChecksum(string input12Digits)
        {
            if (input12Digits.Length != 12)
                throw new ArgumentException("EAN-13 base must be 12 digits.");

            int sumOdd = 0;
            int sumEven = 0;

            for (int i = 0; i < 12; i++)
            {
                int digit = input12Digits[i] - '0';

                if (i % 2 == 0)
                    sumOdd += digit;
                else
                    sumEven += digit;
            }

            int total = sumOdd + sumEven * 3;
            int mod = total % 10;

            return mod == 0 ? 0 : 10 - mod;
        }
    }
}

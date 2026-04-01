using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using SharedKernel.Repositories;

namespace Identity.Infrastructure.Queries;

public sealed class UserQueries : IUserQueries
{
    private readonly IDapperExecutor _dapper;

    public UserQueries(IDapperExecutor dapper)
    {
        _dapper = dapper;
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT
                               u.Id,
                               u.Username,
                               u.FullName,
                               u.Email,
                               u.PhoneNumber,
                               u.Address,
                               u.IsActive,
                               u.CreatedAt,
                               u.UpdatedAt,
                               r.RoleName
                           FROM Users u
                           LEFT JOIN UserRoles ur ON ur.UserId = u.Id
                           LEFT JOIN Roles r ON r.Id = ur.RoleId AND r.IsDeleted = 0
                           WHERE u.IsDeleted = 0
                           ORDER BY u.Id;
                           """;

        var rows = await _dapper.QueryAsync<UserWithRoleRow>(sql, cancellationToken: cancellationToken);

        var users = new Dictionary<int, UserDto>();

        foreach (var row in rows)
        {
            if (!users.TryGetValue(row.Id, out var dto))
            {
                dto = new UserDto
                {
                    Id = row.Id,
                    Username = row.Username ?? string.Empty,
                    FullName = row.FullName ?? string.Empty,
                    Email = row.Email ?? string.Empty,
                    PhoneNumber = row.PhoneNumber,
                    Address = row.Address,
                    IsActive = row.IsActive,
                    CreatedAt = row.CreatedAt,
                    UpdatedAt = row.UpdatedAt,
                    Roles = new List<string>()
                };

                users.Add(row.Id, dto);
            }

            if (!string.IsNullOrWhiteSpace(row.RoleName) && !dto.Roles.Contains(row.RoleName))
            {
                dto.Roles.Add(row.RoleName);
            }
        }

        return users.Values.OrderBy(u => u.Id).ToList();
    }

    private sealed class UserWithRoleRow
    {
        public int Id { get; init; }
        public string? Username { get; init; }
        public string? FullName { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Address { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string? RoleName { get; init; }
    }
}


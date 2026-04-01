using Identity.Application.DTOs;

namespace Identity.Application.Interfaces;

public interface IUserQueries
{
    Task<IReadOnlyList<UserDto>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default);
}


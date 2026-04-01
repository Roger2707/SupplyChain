namespace Identity.Application.DTOs;

public class CreateUserDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public int? WarehouseId { get; set; }
    public List<int> RoleIds { get; set; } = new();
}


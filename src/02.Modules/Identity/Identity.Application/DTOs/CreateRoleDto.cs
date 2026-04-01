namespace Identity.Application.DTOs;

public class CreateRoleDto
{
    public required string RoleName { get; set; }
    public string? Description { get; set; }
}


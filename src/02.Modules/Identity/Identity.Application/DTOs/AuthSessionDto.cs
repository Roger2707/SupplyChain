namespace Identity.Application.DTOs;

public class AuthSessionDto
{
    public AuthResponseDto AuthResponse { get; set; } = null!;
    public string RefreshToken { get; set; } = string.Empty;
}

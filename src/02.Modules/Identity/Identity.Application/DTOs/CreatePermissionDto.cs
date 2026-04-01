namespace Identity.Application.DTOs
{
    public class CreatePermissionDto
    {
        public int Id { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

namespace DocumentManagementBackend.Models.DTOs;

public class RegisterDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? ServerName { get; set; }
    public string? Password { get; set; }
}
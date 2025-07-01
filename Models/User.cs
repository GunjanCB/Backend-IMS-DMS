namespace DocumentManagementBackend.Models;

public class User
{
    public int Id { get; set; }
    public string? ServerName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }

    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }
        
    }


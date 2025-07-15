namespace DocumentManagementBackend.Models
{
    public class PendingUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ServerName { get; set; }
        public string PasswordHash { get; set; }
        public string OtpCode { get; set; }
        public DateTime OtpExpiry { get; set; }
    }
}
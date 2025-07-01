namespace DocumentManagementBackend.Models
{
    public class UserOtp
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public DateTime ExpiryTime { get; set; }
    }
}
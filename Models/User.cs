namespace DocumentManagementBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string ServerName { get; set; } = null;
        public string Username { get; set; } = null;
        public string Password { get; set; } = null;

        
    }

}
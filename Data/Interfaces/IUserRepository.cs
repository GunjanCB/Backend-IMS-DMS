using DocumentManagementBackend.Models;
using System.Threading.Tasks;

namespace DocumentManagementBackend.Data.Interfaces
{

  public interface IUserRepository
  {
    Task AddUserAsync(User user);
    Task<User?> GetUserByUsernameorEmailAsync(string identifier);
    Task<User?> GetUserByIdentifierAsync(string identifier);

    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> SaveChangesAsync();

    Task SaveOtpAsync(string email, string otpCode, DateTime expiry);
    Task<bool> VerifyOtpAsync(string email, string otpCode);
    Task<User?> GetUserByIdAsync(int id);
      
    }
}


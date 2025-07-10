using DocumentManagementBackend.Data.Interfaces;
using DocumentManagementBackend.Data;
using DocumentManagementBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DocumentManagementBackend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User?> ValidateUserAsync(string identifier, string password)
{
    
    return await _context.Users
         .FirstOrDefaultAsync(u => (u.Username == identifier || u.Email == identifier) 
                                   && u.Password == password);
}


        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByUsernameorEmailAsync(string identifier)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
{
    return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
}
public async Task<User?> GetUserByIdentifierAsync(string identifier)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);
}


        public async Task SaveOtpAsync(string email, string otpCode, DateTime expiry)
        {
            var existingOtp = await _context.UserOtps.FirstOrDefaultAsync(o => o.Email == email);
            if (existingOtp != null)
            {
                existingOtp.OtpCode = otpCode;
                existingOtp.ExpiryTime = expiry;
            }
            else
            {
                _context.UserOtps.Add(new UserOtp { Email = email, OtpCode = otpCode, ExpiryTime = expiry });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> VerifyOtpAsync(string email, string otpCode)
        {
            var otp = await _context.UserOtps.FirstOrDefaultAsync(o => o.Email == email && o.OtpCode == otpCode);
            if (otp == null || otp.ExpiryTime < DateTime.UtcNow)
                return false;

            _context.UserOtps.Remove(otp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}

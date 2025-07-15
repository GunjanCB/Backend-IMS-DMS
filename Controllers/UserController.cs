using Microsoft.AspNetCore.Mvc;
using DocumentManagementBackend.Models;
using DocumentManagementBackend.Models.DTOs;
using DocumentManagementBackend.Data.Interfaces;
using DocumentManagementBackend.Services;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;
using DocumentManagementBackend.Data;
using Microsoft.EntityFrameworkCore;




namespace DocumentManagementBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly DataContext _context;
        public UserController(IUserRepository userRepository, IEmailService emailService, IConfiguration config, DataContext context)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;
            _context = context;
        }

        private string GenerateOtp(int length = 6)
        {
            var random = new Random();
            var otp = new char[length];
            for (int i = 0; i < length; i++)
                otp[i] = (char)('0' + random.Next(0, 10));
            return new string(otp);

        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] OtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required.");

            var otpCode = GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            await _userRepository.SaveOtpAsync(dto.Email, otpCode, expiry);

            _emailService.SendEmail(dto.Email, "Your OTP Code", $"Your Otp Code is {otpCode}");
            return Ok(new { message = "OTP sent to your email" });

        }


        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest("Email and OTP Code are required.");

            var isValid = await _userRepository.VerifyOtpAsync(dto.Email, dto.Code);
            if (!isValid)
                return Unauthorized("Invalid or expired OTP.");

            return Ok(new { message = "OTP verified successfully" });
        }

        [HttpPost("pre-register")]
        public async Task<IActionResult> PreRegister([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.ServerName))
                return BadRequest("All fields are required");

            // Check if user exists in User or PendingUsers
            if (await _userRepository.GetUserByEmailAsync(dto.Email) != null)
                return BadRequest("Email already registered");

            var existingPending = await _context.PendingUsers.FirstOrDefaultAsync(p => p.Email == dto.Email);
            if (existingPending != null)
                _context.PendingUsers.Remove(existingPending);

            var passwordHasher = new PasswordHasher<PendingUser>();
            var hashedPassword = passwordHasher.HashPassword(null!, dto.Password);
            var otp = GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            var pendingUser = new PendingUser
            {
                Email = dto.Email,
                Username = dto.Username,
                ServerName = dto.ServerName,
                PasswordHash = hashedPassword,
                OtpCode = otp,
                OtpExpiry = expiry
            };

            _context.PendingUsers.Add(pendingUser);
            await _context.SaveChangesAsync();

            _emailService.SendEmail(dto.Email, "Verify Your Account", $"Your OTP is {otp}");

            return Ok(new { message = "OTP sent to email. Verify to complete registration." });
        }



        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] OtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest("Email and OTP Code are required.");

            var pendingUser = await _context.PendingUsers.FirstOrDefaultAsync(p => p.Email == dto.Email);
            if (pendingUser == null)
                return Unauthorized("No pending registration found for this email.");

            if (pendingUser.OtpCode != dto.Code || pendingUser.OtpExpiry < DateTime.UtcNow)
                return Unauthorized("Invalid or expired OTP.");

            // (Race condition) re check email reg again or not
            if (await _userRepository.GetUserByEmailAsync(pendingUser.Email) != null)
                return BadRequest("Email already registered");

            var newUser = new User
            {
                Username = pendingUser.Username,
                Email = pendingUser.Email,
                ServerName = pendingUser.ServerName,
                PasswordHash = pendingUser.PasswordHash
            };

            await _userRepository.AddUserAsync(newUser);
            // Del from PEnding
            _context.PendingUsers.Remove(pendingUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration complete. You can now login" });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userRepository.GetUserByIdentifierAsync(dto.Identifier);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var passwordHasher = new PasswordHasher<User>();
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (verificationResult != PasswordVerificationResult.Success)
                return Unauthorized(new { message = "Invalid username or password" });

            // JWT token Generate
            var jwtSection = _config.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSection["SecretKey"]);
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            // 3. Return user info + token
            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                },
                token = jwtToken
            });
        }
        [Authorize]
            [HttpPost("change-password")]
            public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
            {
                if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                {
                    return BadRequest("All field are required.");
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return BadRequest("New and confirm password do not match.");
                }

                // Get logged in user from jwt
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized();

                var user = await _userRepository.GetUserByIdAsync(int.Parse(userId));
                if (user == null)
                    return Unauthorized("User not found");

                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, dto.CurrentPassword);

                if (result != PasswordVerificationResult.Success)
                {
                    return BadRequest("Current password is incorrect.");
                }

                // Hash n update new pw
                user.PasswordHash = passwordHasher.HashPassword(user, dto.NewPassword);
                await _userRepository.SaveChangesAsync();

                return Ok(new { message = "Password Updated Successfully." });

            
            }
    }
}
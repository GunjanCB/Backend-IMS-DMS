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


namespace DocumentManagementBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public UserController(IUserRepository userRepository, IEmailService emailService, IConfiguration config)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;
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


        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.ServerName) ||
                string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("All Fields are required.");

            }
            // Save user to DB
            await _userRepository.AddUserAsync(user);

            // Generate OTP
            var otpCode = GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            // Save OTP linked to user's Email
            await _userRepository.SaveOtpAsync(user.Email, otpCode, expiry);

            // Send OTP Email
            _emailService.SendEmail(user.Email, "Your OTP Code", $"Your OTP Code is {otpCode}");

            return Ok(new { message = "User registered successfully. OTP sent to your Email." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userRepository.ValidateUserAsync(dto.Identifier, dto.Password);
            if (user == null)
                return BadRequest(new { message = "Invalid username or password" });

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
                Expires = DateTime.UtcNow.AddHours(2),
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
    }
}
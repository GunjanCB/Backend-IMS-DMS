using Microsoft.AspNetCore.Mvc;
using DocumentManagementBackend.Models;
using DocumentManagementBackend.Models.DTOs;
using DocumentManagementBackend.Data.Interfaces;
using DocumentManagementBackend.Services;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagementBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        public UserController(IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
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
public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
{
    if (loginDto == null || string.IsNullOrEmpty(loginDto.Identifier) || string.IsNullOrEmpty(loginDto.Password))
        return BadRequest("Username or Email and password are required.");

    var user = await _userRepository.GetUserByIdentifierAsync(loginDto.Identifier);

    if (user == null || user.Password != loginDto.Password)
        return Unauthorized("Invalid username/email or password.");

    return Ok(new { message = "Login successful", user });
}

        }
    }

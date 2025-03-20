using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTO;
using NLog;
using BusinessLayer.Interface;
using MiddleWare.Email;
using MiddleWare.HashingAlgo;
using MiddleWare.JwtHelper;
using ModelLayer.Model;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using MiddleWare.RabbitMQClient;
namespace HelloGreetingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IUserBL _userBL;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly IConfiguration _configuration;
        private readonly SMTP _smtp;
        private readonly IRabbitMQService _rabbitMQService;

        public UserController(IUserBL userBL, JwtTokenHelper _jwtTokenHelper, SMTP _smtp, IConfiguration configuration, IRabbitMQService _rabbitMQService)
        {
            this._jwtTokenHelper = _jwtTokenHelper;
            this._smtp = _smtp;
            _userBL = userBL ?? throw new ArgumentNullException(nameof(userBL), "UserBL cannot be null."); // Ensures userBL is not null
            _configuration = configuration;
            this._rabbitMQService = _rabbitMQService;
        }

        [HttpPost("registerUser")]
        public IActionResult Register(RegisterDTO registerDTO)
        {
            try
            {
                _logger.Info($"Register attempt for email: {registerDTO.Email}");

                var newUser = _userBL.RegistrationBL(registerDTO);

                if (newUser == null)
                {
                    _logger.Warn($"Registration failed. Email already exists: {registerDTO.Email}");
                    return Conflict(new { Success = false, Message = "User with this email already exists." });
                }

                _logger.Info($"User registered successfully: {registerDTO.Email}");
                string email = registerDTO.Email;
                _rabbitMQService.SendMessage(email + ", You have succeffuly Registered & LoggedIn!!");
                return Created("user registered", new { Success = true, Message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Registration failed for {registerDTO.Email}");
                return BadRequest(new { Success = false, Message = "Registration failed.", Error = ex.Message });
            }
        }

        [HttpPost("loginUser")]
        public IActionResult Login(LoginDTO loginDTO)
        {
            try
            {
                _logger.Info($"Login attempt for user: {loginDTO.Email}");

                var (user, token) = _userBL.LoginnUserBL(loginDTO);

                if (user == null || string.IsNullOrEmpty(token))
                {
                    _logger.Warn($"Invalid login attempt for user: {loginDTO.Email}");
                    return Unauthorized(new { Success = false, Message = "Invalid username or password." });
                }

                _logger.Info($"User {loginDTO.Email} logged in successfully.");
                _rabbitMQService.ReceiveMessage();
                return Ok(new
                {
                    Success = true,
                    Message = "Login Successful.",
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Login failed for {loginDTO.Email}");
                return BadRequest(new { Success = false, Message = "Login failed.", Error = ex.Message });
            }
        }

        // Forgot Password API (Step 1: Sends Reset Token via Email)
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { message = "Email is required." }); // Returns 400 Bad Request if email is empty
                }

                bool result = _userBL.ValidateEmail(request.Email); // Checks if the email exists in the database

                Console.WriteLine("result: " + result);

                if (!result)
                {
                    return Ok(new { message = "Not a valid email" }); // Returns a success message but informs user of invalid email
                }

                string mail = request.Email;

                // Generate password reset token
                var resetToken = _jwtTokenHelper.GeneratePasswordResetToken(mail);

                // Email details
                string subject = "Reset Your Password";
                string body = $"Click the link to reset your password: \n https://HelloGreetingApp.com/reset-password?token={resetToken}";

                // Sends email with password reset link
                _smtp.SendEmailAsync(request.Email, subject, body);

                return Ok(new { message = "Password reset email has been sent." }); // Returns success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error occurred while processing the password reset", error = ex.Message }); // Returns 400 Bad Request on error
            }
        }

        // Reset Password API (Step 2: Verify Token & Update Password)
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Token))
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid token." }); // Returns 400 Bad Request if token is empty

                var principal = _jwtTokenHelper.ValidateToken(model.Token); // Validates JWT token
                if (principal == null)
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid or expired token." }); // Returns 400 Bad Request if token is invalid

                // Extract email from token
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value
                              ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                var isResetTokenClaim = principal.FindFirst("isPasswordReset")?.Value; // Checks if token is for password reset

                if (string.IsNullOrEmpty(isResetTokenClaim) || isResetTokenClaim != "true")
                {
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid reset token." }); // Returns 400 if not a valid reset token
                }

                if (emailClaim != model.Email)
                {
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid Email." }); // Returns 400 if email does not match token
                }

                if (string.IsNullOrEmpty(emailClaim))
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Email not found in token." }); // Returns 400 if email is missing in token

                var user = _userBL.GetByEmail(emailClaim); // Fetch user details
                if (user == null)
                    return NotFound(new ResponseModel<string> { Success = false, Message = "User not found" }); // Returns 404 if user not found

                // Securely hash the new password
                string password = HashingMethods.HashPassword(model.NewPassword);

                _userBL.UpdateUserPassword(model.Email, password); // Updates the password in the database

                return Ok(new ResponseModel<string> { Success = true, Message = "Password reset successfully" }); // Returns success message
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseModel<string> { Success = false, Message = e.Message }); // Returns 400 if an error occurs
            }
        }

    }
}
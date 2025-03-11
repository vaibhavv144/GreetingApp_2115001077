using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTO;
using NLog;
using BusinessLayer.Interface;

namespace HelloGreetingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IUserBL _userBL;

        public UserController(IUserBL userBL)
        {
            _userBL = userBL;
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

    }
}
using System;
using ModelLayer.DTO;
using Microsoft.Extensions.Logging;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using BusinessLayer.Interface;
using MiddleWare.JwtHelper;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly ILogger<UserBL> _logger;
        private readonly IUserRL _userRL;
        private readonly JwtTokenHelper _jwtTokenHelper;

        public UserBL(IUserRL userRL, ILogger<UserBL> logger, JwtTokenHelper jwtTokenHelper)
        {
            _logger = logger;
            _userRL = userRL;
            _jwtTokenHelper = jwtTokenHelper; // ✅ Assign JWT Helper
        }

        public UserEntity RegistrationBL(RegisterDTO registerDTO)
        {
            try
            {
                _logger.LogInformation("Attempting to register user: {Email}", registerDTO.Email);
                var result = _userRL.Registration(registerDTO);
                if (result != null)
                {
                    _logger.LogInformation("User registration successful for: {Email}", registerDTO.Email);
                }
                else
                {
                    _logger.LogWarning("User registration failed for: {Email}", registerDTO.Email);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for {Email}", registerDTO.Email);
                throw;
            }
        }

        public (UserEntity user, string token) LoginnUserBL(LoginDTO loginDTO)
        {
            try
            {
                _logger.LogInformation("Attempting to log in user: {Email}", loginDTO.Email);
                var user = _userRL.LoginnUserRL(loginDTO);

                if (user != null)
                {
                    _logger.LogInformation("Login successful for user: {Email}", loginDTO.Email);
                    var token = _jwtTokenHelper.GenerateToken(user);
                    return (user, token);
                }

                _logger.LogWarning("Login failed for user: {Email}", loginDTO.Email);
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginDTO.Email);
                throw;
            }
        }
    }
}
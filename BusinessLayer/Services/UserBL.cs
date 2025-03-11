
using System;
using ModelLayer.DTO;
using Microsoft.Extensions.Logging; // ✅ Use Microsoft ILogger
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using BusinessLayer.Interface;
namespace BusinessLayer.Services;
public class UserBL : IUserBL
{
    private readonly ILogger<UserBL> _logger; // ✅ Correct logging
    private readonly IUserRL _userRL;

    public UserBL(IUserRL userRL, ILogger<UserBL> logger)
    {
        _logger = logger;
        _userRL = userRL;
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

    public UserEntity LoginnUserBL(LoginDTO loginDTO)
    {
        try
        {
            _logger.LogInformation("Attempting to log in user: {Email}", loginDTO.Email);

            var result = _userRL.LoginnUserRL(loginDTO);
            if (result != null)
            {
                _logger.LogInformation("Login successful for user: {Email}", loginDTO.Email);
            }
            else
            {
                _logger.LogWarning("Login failed for user: {Email}", loginDTO.Email);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", loginDTO.Email);
            throw;
        }
    }
}

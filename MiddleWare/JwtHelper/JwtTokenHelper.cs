﻿using Microsoft.IdentityModel.Tokens;
using System.Text;
//using ModelLayer.Entity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
//using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace MiddleWare.JwtHelper
{
    public class JwtTokenHelper
    {
        private readonly IConfiguration _configuration;
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;


        public JwtTokenHelper(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            _key = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }

        public string GenerateToken(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");

            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"] ?? throw new ArgumentException("JWT SecretKey missing."));
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique identifier
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public string GeneratePasswordResetToken(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");

            var keyString = _configuration["JwtSettings:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new ArgumentException("JWT SecretKey is missing or empty.");

            var key = Encoding.UTF8.GetBytes(keyString);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Email, email),
        new Claim("isPasswordReset", "true")
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Validate Token and Extract Claims
        public ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token), "Token cannot be null or empty.");

            var keyString = _configuration["JwtSettings:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new ArgumentException("JWT SecretKey is missing or empty.");

            var key = Encoding.UTF8.GetBytes(keyString);
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken validatedToken);

                //_logger.LogInformation("✅ Token successfully validated. Claims:");
                foreach (var claim in principal.Claims)
                {
                    //_logger.LogInformation("🔹 {Type}: {Value}", claim.Type, claim.Value);
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                //_logger.LogWarning("❌ Token expired.");
                return null;
            }
            catch (Exception ex)
            {
                //_logger.LogError("❌ Token validation failed: {Message}", ex.Message);
                return null;
            }
        }

    }
}
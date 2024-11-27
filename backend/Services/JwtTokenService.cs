using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace grifindo_lms_api.Services
{
    public class JwtTokenService
    {
        private static readonly string SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
             ?? throw new ArgumentNullException("Environment variable 'JWT_SECRET_KEY' is not set.");

        private static readonly string Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new ArgumentNullException("Environment variable 'JWT_ISSUER' is not set.");

        public static string GenerateToken(string employeeNumber, string role, int expireMinutes = 60)
        {
            // Add claims, including the user's role
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, employeeNumber),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            // Serialize and return the token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

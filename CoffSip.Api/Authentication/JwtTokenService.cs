using CoffSip.Application.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoffSip.Api.Authentication
{
    public sealed record LoginTokenResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc);

    // Generate short-lived JWT access tokens for authenticated demo users.
    public sealed class JwtTokenService(IConfiguration configuration)
    {
        public LoginTokenResponse CreateToken(AuthenticatedUser user)
        {
            var issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is missing.");

            var audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT audience is missing.");

            var key = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is missing.");

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(30);

            var claims = new[]
            {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.UserId.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString("N"))
        };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials);

            return new LoginTokenResponse(
                new JwtSecurityTokenHandler().WriteToken(token),
                "Bearer",
                expiresAt);
        }
    }
}

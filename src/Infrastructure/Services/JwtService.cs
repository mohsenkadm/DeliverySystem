using DeliverySystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DeliverySystem.Infrastructure.Services;

/// <summary>خدمة توليد والتحقق من توكن JWT</summary>
public class JwtService(IConfiguration config) : IJwtService
{
    private readonly string _secret = config["JwtSettings:SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey is not configured");
    private readonly string _issuer = config["JwtSettings:Issuer"] ?? "DeliverySystem";
    private readonly string _audience = config["JwtSettings:Audience"] ?? "DeliverySystemAPI";
    private readonly int _expirationDays = int.Parse(config["JwtSettings:ExpirationDays"] ?? "7");

    /// <summary>توليد توكن JWT جديد للمستخدم</summary>
    public string GenerateToken(int userId, string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _issuer, audience: _audience, claims: claims,
            expires: DateTime.UtcNow.AddDays(_expirationDays),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>استخراج معرف المستخدم من التوكن</summary>
    public int? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        var claim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && int.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>استخراج دور المستخدم من التوكن</summary>
    public string? GetRoleFromToken(string token)
        => ValidateToken(token)?.FindFirst(ClaimTypes.Role)?.Value;

    private ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = key,
                ValidateIssuer = true, ValidIssuer = _issuer,
                ValidateAudience = true, ValidAudience = _audience,
                ValidateLifetime = true
            }, out _);
        }
        catch { return null; }
    }
}

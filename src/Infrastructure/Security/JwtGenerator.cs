using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Common.Ddd.Domain.Values;

using Domain.Security;
using Domain.Security.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class JwtGenerator : IJwtGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expireMinutes;

    public JwtGenerator(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]!;
        _issuer = configuration["Jwt:Issuer"]!;
        _audience = configuration["Jwt:Audience"]!;
        _expireMinutes = int.Parse(configuration["Jwt:ExpireMinutes"]!);
    }

    public Result<AuthContext> GenerateToken(UserInfo userInfo)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
            new Claim(ClaimTypes.Name, userInfo.Username),
            new Claim(ClaimTypes.Role, userInfo.Role),
            new Claim("permission", GetPermissionByRole(userInfo.Role)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
            signingCredentials: credentials
        );

        return new AuthContext
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserInfo = userInfo,
            Role = userInfo.Role
        };
    }

    public async Task<Result<AuthContext>> GenerateTokenAsync(UserInfo userInfo)
    {
        return await Task.FromResult(GenerateToken(userInfo));
    }

    public Result<ClaimsPrincipal?> ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return Result<ClaimsPrincipal?>.Failure(Errors.Authentication.InvalidCredentials());
        }
    }

    public async Task<Result<ClaimsPrincipal?>> ValidateTokenAsync(string token)
    {
        return await Task.FromResult(ValidateToken(token));
    }

    private string GetPermissionByRole(string role)
    {
        return role.ToLower() switch
        {
            "admin" => "database.admin",
            "user" => "database.query",
            _ => "database.query"
        };
    }
}
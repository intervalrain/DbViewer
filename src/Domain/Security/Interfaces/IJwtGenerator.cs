using System.Security.Claims;

using Common.Ddd.Domain.Values;

namespace Domain.Security.Interfaces;

public interface IJwtGenerator
{
    Result<AuthContext> GenerateToken(UserInfo userInfo);
    Task<Result<AuthContext>> GenerateTokenAsync(UserInfo userInfo);
    Result<ClaimsPrincipal?> ValidateToken(string token);
    Task<Result<ClaimsPrincipal?>> ValidateTokenAsync(string token);
}
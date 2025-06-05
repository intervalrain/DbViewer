using Application.Contracts.Dtos;
using Common.Ddd.Domain.Values;

namespace Application.Contracts.Services;

public interface IAuthService
{
    Task<Result<AuthResultDto>> LoginAsync(LoginDto loginDto);
    Task<Result> LogoutAsync();
    Task<Result<bool>> ValidateTokenAsync(string token);
    bool IsAuthenticated { get; }
    string? CurrentToken { get; }
    string? CurrentUser { get; }
    string? CurrentRole { get; }
} 
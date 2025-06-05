using System.Security.Claims;
using Application.Contracts.Dtos;
using Application.Contracts.Services;
using AutoMapper;
using Common.Ddd.Domain.Values;
using Common.Extensions;
using Domain.Security;
using Domain.Security.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IJwtGenerator _jwtGenerator;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;

    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentToken);
    public string? CurrentToken { get; private set; }
    public string? CurrentUser { get; private set; }
    public string? CurrentRole { get; private set; }

    public AuthService(IJwtGenerator jwtGenerator, ILogger<AuthService> logger, IMapper mapper)
    {
        _jwtGenerator = jwtGenerator;
        _logger = logger;
        _mapper = mapper;
    }

    public Task<Result<AuthResultDto>> LoginAsync(LoginDto input)
    {
        return ValidateUserCredentialsAsync(input.Username, input.Password)
            .BindAsync(async isValid => isValid
                ? (await GetUserInfoAsync(input.Username))
                : Result<UserInfoDto>.Failure(Errors.Authentication.InvalidCredentials()))
            .Map(_mapper.Map<UserInfoDto, UserInfo>)
            .BindAsync(_jwtGenerator.GenerateTokenAsync)
            .Tap(auth => _logger.LogInformation("用戶 {Username} 登入成功", auth.UserInfo?.Username))
            .Tap(auth => UpdateCurrentUser(_mapper.Map<UserInfo, UserInfoDto>(auth?.UserInfo!), auth?.Token!))
            .Map(context => new AuthResultDto
            {
                IsSuccess = true,
                Token = context.Token,
                Username = context.UserInfo?.Username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
    }

    public Task<Result> LogoutAsync()
    {
        CurrentToken = null;
        CurrentUser = null;
        CurrentRole = null;
        
        _logger.LogInformation("用戶登出");
        return Task.FromResult(Result.Success());
    }

    public Task<Result<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            var result = _jwtGenerator.ValidateToken(token);
            if (result.IsSuccess)
            {
                // 更新當前用戶狀態
                CurrentToken = token;
                CurrentUser = result.Value?.FindFirst(ClaimTypes.Name)?.Value;
                CurrentRole = result.Value?.FindFirst(ClaimTypes.Role)?.Value;
                return Task.FromResult(Result<bool>.Success(true));
            }
            return Task.FromResult(Result<bool>.Success(false));
        }
        catch
        {
            return Task.FromResult(Result<bool>.Failure(Errors.Authentication.InvalidCredentials()));
        }
    }

    private Task<Result<bool>> ValidateUserCredentialsAsync(string username, string password)
    {
        return Task.FromResult(Result<bool>.Success(username == "admin" && password == "1q2w3E*"));
    }

    private Task<Result<UserInfoDto>> GetUserInfoAsync(string username)
    {
        var user = new UserInfoDto
        {
            Username = username, 
            Role = username == "admin" ? "admin" : "user"
        };
        return Task.FromResult(Result<UserInfoDto>.Success(user));
    }

    private Result<bool> UpdateCurrentUser(UserInfoDto info, string token)
    {
        CurrentToken = token;
        CurrentUser = info.Username;
        CurrentRole = info.Role;

        return Result<bool>.Success(true);
    }
} 
using Application.Contracts.Dtos;

using AutoMapper;

using Domain.Security;

using Domain.Security.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Presentation.WebApi.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IMapper _mapper;

    public AuthController(ILogger<AuthController> logger, IJwtGenerator jwtGenerator, IMapper mapper)
    {
        _jwtGenerator = jwtGenerator;
        _mapper = mapper;

        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!await ValidateUserCredentials(request.Username, request.Password))
        {
            return Unauthorized("用戶名或密碼錯誤");
        }

        var user = await GetUserInfo(request.Username);
        var context = await _jwtGenerator.GenerateTokenAsync(_mapper.Map<UserInfoDto, UserInfo>(user));

        _logger.LogInformation("用戶 {Username} 登入成功", request.Username);

        return Ok(new LoginResponse
        (
            context.Value.Token!,
            context.Value.UserInfo!.Id,
            context.Value.UserInfo!.Username,
            context.Value.UserInfo!.Role,
            DateTime.UtcNow.AddMinutes(60)
        ));
    }

    private Task<bool> ValidateUserCredentials(string username, string password)
    {
        return Task.FromResult(username == "admin" && password == "1q2w3E*");
    }

    private Task<UserInfoDto> GetUserInfo(string username)
    {
        var user = new UserInfoDto
        {
            Username = username,
            Role = username == "admin" ? "admin" : "user"
        };
        return Task.FromResult(user);
    }
}
namespace Presentation.WebApi.Models;

public record LoginResponse(
    string Token,
    Guid UserId,
    string Username,
    string Role,
    DateTime ExpiresAt
);
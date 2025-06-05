using Common.Ddd.Domain.Entities;

namespace Domain.Security;

public class UserInfo(string username, string role) : Entity<Guid>(Guid.NewGuid())
{
    public string Username { get; set; } = username;
    public string Role { get; set; } = role;
}
namespace Domain.Security;

public class AuthContext
{
    public string? Token { get; set; }
    public UserInfo? UserInfo { get; set; }
    public string? Role { get; set; }
}
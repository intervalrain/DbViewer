using System.ComponentModel;

namespace Application.Contracts.Dtos;

public class ConnectionInfoDto
{
    [DefaultValue("pg")]
    public string DisplayName { get; set; } = "";

    [DefaultValue("localhost")]
    public string Host { get; set; } = "";

    [DefaultValue("5432")]
    public int Port { get; set; } = 5432;

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    [DefaultValue("postgres")]
    public string DefaultDatabase { get; set; } = "";

    [DefaultValue("true")]
    public bool SavePassword { get; set; } = true;

    public DateTime LastUsed { get; set; } = DateTime.Now;
} 
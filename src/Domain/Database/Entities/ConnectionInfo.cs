using System.Text.Json;

using Common.Ddd.Domain.Entities;

namespace Domain.Database.Entities;

public class ConnectionInfo() : Entity<Guid>(Guid.NewGuid())
{
    public string DisplayName { get; set; } = "";
    
    public string Host { get; set; } = "";
    
    public int Port { get; set; } = 5432;
    
    public string Username { get; set; } = "";
    
    public string Password { get; set; } = "";
    
    public string DefaultDatabase { get; set; } = "";
    
    public bool SavePassword { get; set; } = false;
    
    public DateTime LastUsed { get; set; } = DateTime.Now;
    
    public string BuildConnectionString(string? database = null)
    {
        var dbName = database ?? DefaultDatabase;
        return $"Host={Host};Port={Port};Database={dbName};Username={Username};Password={Password};";
    }
    
    public string BuildDisplayConnectionString(string? database = null)
    {
        var dbName = database ?? DefaultDatabase;
        return $"Host={Host};Port={Port};Database={dbName};Username={Username};";
    }
    
    public ConnectionInfo CloneWithoutPassword()
    {
        return new ConnectionInfo()
        {
            DisplayName = DisplayName,
            Host = Host,
            Port = Port,
            Username = Username,
            Password = "",
            DefaultDatabase = DefaultDatabase,
            SavePassword = false,
            LastUsed = LastUsed
        };
    }
    
    public static ConnectionInfo? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ConnectionInfo>(json);
        }
        catch
        {
            return null;
        }
    }
    
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
} 
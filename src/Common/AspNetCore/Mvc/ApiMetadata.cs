namespace Common.AspNetCore.Mvc;

/// <summary>
/// API response metadata
/// </summary>
public class ApiMetadata
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
    public string? Version { get; set; }
    public Dictionary<string, object>? Additional { get; set; }
}
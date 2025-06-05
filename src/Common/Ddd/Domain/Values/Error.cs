namespace Common.Ddd.Domain.Values;

public sealed record Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }
    public Dictionary<string, object> Metadata { get; }

    private Error(string code, string description, ErrorType type, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Description = description;
        Type = type;
        Metadata = metadata ?? [];
    }

    public static Error Failure(string code, string description, Dictionary<string, object>? metadata = null)
        => new(code, description, ErrorType.Failure, metadata);

    public static Error Validation(string code, string description, Dictionary<string, object>? metadata = null)
        => new(code, description, ErrorType.Validation, metadata);

    public static Error NotFound(string code, string description, Dictionary<string, object>? metadata = null)
        => new(code, description, ErrorType.NotFound, metadata);

    public static Error Conflict(string code, string description, Dictionary<string, object>? metadata = null)
        => new(code, description, ErrorType.Conflict, metadata);

    public static Error Unauthorized(string code, string description, Dictionary<string, object>? metadata = null)
        => new(code, description, ErrorType.Unauthorized, metadata);

    public static Error Forbidden(string code, string description, Dictionary<string, object>? metadata = null)
        => new(code, description, ErrorType.Forbidden, metadata);

    public Error WithMetadata(string key, object value)
    {
        var newMetaadata = new Dictionary<string, object>(Metadata) { [key] = value };
        return new Error(Code, Description, Type, newMetaadata);
    }

    public Error WithMetadata(Dictionary<string, object> metadata)
    {
        var newMetadata = new Dictionary<string, object>(Metadata);
        foreach (var kvp in metadata)
        {
            newMetadata[kvp.Key] = kvp.Value;
        }
        return new Error(Code, Description, Type, newMetadata);
    }
}
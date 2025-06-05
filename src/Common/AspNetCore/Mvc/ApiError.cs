using Common.Ddd.Domain.Values;

namespace Common.AspNetCore.Mvc;


/// <summary>
/// API error representation
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Field { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }

    public static ApiError FromError(Error error, string? field = null)
    {
        return new ApiError
        {
            Code = error.Code,
            Description = error.Description,
            Field = field,
            Metadata = error.Metadata.Any() ? error.Metadata : null
        };
    }
}
namespace Common.AspNetCore.Mvc;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<ApiError>? Errors { get; set; }
    public ApiMetadata? Metadata { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null, ApiMetadata? metadata = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Metadata = metadata
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<ApiError>? errors = null, ApiMetadata? metadata = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Metadata = metadata
        };
    }
}

/// <summary>
/// Non-generic API response
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse SuccessResponse(string? message = null, ApiMetadata? metadata = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message ?? "Operation completed successfully",
            Metadata = metadata
        };
    }

    public new static ApiResponse ErrorResponse(string message, List<ApiError>? errors = null, ApiMetadata? metadata = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            Metadata = metadata
        };
    }
}
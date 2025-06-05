using System.Net;

using Common.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Base controller with Result support
/// </summary>
[ApiController]
public abstract class ApiController : ControllerBase
{
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.SuccessResponse(data, message, CreateMetadata()));
    }

    protected IActionResult Success(string? message = null)
    {
        return Ok(ApiResponse.SuccessResponse(message, CreateMetadata()));
    }

    protected IActionResult Created<T>(T data, string location, string? message = null)
    {
        return StatusCode((int)HttpStatusCode.Created,
            ApiResponse<T>.SuccessResponse(data, message, CreateMetadata()));
    }

    protected IActionResult CreatedAtAction<T>(T data, string actionName, object? routeValues = null, string? message = null)
    {
        var response = ApiResponse<T>.SuccessResponse(data, message, CreateMetadata());
        return CreatedAtAction(actionName, routeValues, response);
    }

    protected IActionResult NoContent(string? message = null)
    {
        return StatusCode((int)HttpStatusCode.NoContent,
            ApiResponse.SuccessResponse(message, CreateMetadata()));
    }

    protected IActionResult BadRequest(string message, List<ApiError>? errors = null)
    {
        return StatusCode((int)HttpStatusCode.BadRequest,
            ApiResponse.ErrorResponse(message, errors, CreateMetadata()));
    }

    protected IActionResult NotFound(string message)
    {
        return StatusCode((int)HttpStatusCode.NotFound,
            ApiResponse.ErrorResponse(message, null, CreateMetadata()));
    }

    protected IActionResult Conflict(string message)
    {
        return StatusCode((int)HttpStatusCode.Conflict,
            ApiResponse.ErrorResponse(message, null, CreateMetadata()));
    }

    protected IActionResult Forbidden(string message)
    {
        return StatusCode((int)HttpStatusCode.Forbidden,
            ApiResponse.ErrorResponse(message, null, CreateMetadata()));
    }

    protected IActionResult Unauthorized(string message)
    {
        return StatusCode((int)HttpStatusCode.Unauthorized,
            ApiResponse.ErrorResponse(message, null, CreateMetadata()));
    }

    protected IActionResult InternalServerError(string message)
    {
        return StatusCode((int)HttpStatusCode.InternalServerError,
            ApiResponse.ErrorResponse(message, null, CreateMetadata()));
    }

    private ApiMetadata CreateMetadata()
    {
        return new ApiMetadata
        {
            RequestId = HttpContext.TraceIdentifier,
            Version = "1.0" // 可以從配置或 Assembly 中獲取
        };
    }
}
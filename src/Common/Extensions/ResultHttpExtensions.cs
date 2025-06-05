using System.Net;
using Common.AspNetCore.Mvc;
using Common.Ddd.Domain.Values;
using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions;

/// <summary>
/// Extension methods to convert Result to HTTP responses
/// </summary>
public static class ResultHttpExtensions
{
    /// <summary>
    /// Convert Result<T> to IActionResult
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result,

        string? successMessage = null,
        HttpStatusCode? successStatusCode = null,
        ApiMetadata? metadata = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.SuccessResponse(result.Value, successMessage, metadata);
            return new ObjectResult(response)
            {
                StatusCode = (int)(successStatusCode ?? HttpStatusCode.OK)
            };
        }

        return result.Errors.ToErrorActionResult(metadata);
    }

    /// <summary>
    /// Convert non-generic Result to IActionResult
    /// </summary>
    public static IActionResult ToActionResult(this Result result,
        string? successMessage = null,
        HttpStatusCode successStatusCode = HttpStatusCode.OK,
        ApiMetadata? metadata = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse.SuccessResponse(successMessage, metadata);
            return new ObjectResult(response)
            {
                StatusCode = (int)successStatusCode
            };
        }

        return result.Errors.ToErrorActionResult(metadata);
    }

    /// <summary>
    /// Convert Result<T> to Created response
    /// </summary>
    public static IActionResult ToCreatedResult<T>(this Result<T> result,
        string location,
        string? successMessage = null,
        ApiMetadata? metadata = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.SuccessResponse(result.Value, successMessage, metadata);
            return new CreatedResult(location, response);
        }

        return result.Errors.ToErrorActionResult(metadata);
    }

    /// <summary>
    /// Convert Result<T> to Created response with route values
    /// </summary>
    public static IActionResult ToCreatedAtActionResult<T>(this Result<T> result,
        string actionName,
        string? controllerName,
        object? routeValues,
        string? successMessage = null,
        ApiMetadata? metadata = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.SuccessResponse(result.Value, successMessage, metadata);
            return new CreatedAtActionResult(actionName, controllerName, routeValues, response);
        }

        return result.Errors.ToErrorActionResult(metadata);
    }

    /// <summary>
    /// Convert errors to appropriate HTTP error response
    /// </summary>
    public static IActionResult ToErrorActionResult(this List<Error> errors, ApiMetadata? metadata = null)
    {
        if (!errors.Any())
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);

        var primaryError = errors.First();
        var (statusCode, message) = GetHttpStatusFromErrorType(primaryError.Type);

        var apiErrors = errors.Select(e => ApiError.FromError(e)).ToList();
        var response = ApiResponse.ErrorResponse(message, apiErrors, metadata);

        return new ObjectResult(response)
        {
            StatusCode = (int)statusCode
        };
    }

    /// <summary>
    /// Convert Result<T> to paginated response
    /// </summary>
    public static IActionResult ToPaginatedResult<T>(this Result<IEnumerable<T>> result,
        int page,
        int pageSize,
        int totalCount,
        string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var metadata = new PaginationMetadata
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            var response = ApiResponse<IEnumerable<T>>.SuccessResponse(result.Value, successMessage, metadata);
            return new OkObjectResult(response);
        }

        return result.Errors.ToErrorActionResult();
    }

    private static (HttpStatusCode statusCode, string message) GetHttpStatusFromErrorType(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => (HttpStatusCode.BadRequest, "Validation failed"),
            ErrorType.NotFound => (HttpStatusCode.NotFound, "Resource not found"),
            ErrorType.Conflict => (HttpStatusCode.Conflict, "Resource conflict"),
            ErrorType.Unauthorized => (HttpStatusCode.Unauthorized, "Authentication required"),
            ErrorType.Forbidden => (HttpStatusCode.Forbidden, "Access denied"),
            ErrorType.Failure => (HttpStatusCode.InternalServerError, "Operation failed"),
            _ => (HttpStatusCode.InternalServerError, "An error occurred")
        };
    }
}
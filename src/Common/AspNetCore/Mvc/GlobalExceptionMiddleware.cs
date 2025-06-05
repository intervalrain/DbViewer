using System.Net;
using System.Text.Json;

using Common.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


/// <summary>
/// Global exception handler for Result pattern
/// </summary>
public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var metadata = new ApiMetadata
            {
                RequestId = context.TraceIdentifier,
                Version = "1.0"
            };

            ApiResponse response = exception switch
            {
                ArgumentException => ApiResponse.ErrorResponse("Invalid argument", null, metadata),
                UnauthorizedAccessException => ApiResponse.ErrorResponse("Unauthorized", null, metadata),
                NotImplementedException => ApiResponse.ErrorResponse("Not implemented", null, metadata),
                TimeoutException => ApiResponse.ErrorResponse("Request timeout", null, metadata),
                _ => ApiResponse.ErrorResponse("Internal server error", null, metadata)
            };

            var statusCode = exception switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                NotImplementedException => HttpStatusCode.NotImplemented,
                TimeoutException => HttpStatusCode.RequestTimeout,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
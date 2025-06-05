namespace Common.Ddd.Domain.Values;

/// <summary>
/// Common errors for typical scenarios
/// </summary>
public static class Errors
{
    public static class General
    {
        public static Error UnexpectedError(string? details = null) =>
            Error.Failure("General.UnexpectedError", details ?? "An unexpected error occurred");

        public static Error ServiceUnavailable(string? service = null) =>
            Error.Failure("General.ServiceUnavailable", $"Service {service ?? "requested"} is unavailable");
    }

    public static class Database
    {
        public static Error ConnectionFailed(string? details = null) =>
            Error.Failure("Database.ConnectionFailed", details ?? "Failed to connect to the database");
    }

    public static class Validation
    {
        public static Error Required(string field) =>
            Error.Validation("Validation.Required", $"{field} is required");

        public static Error InvalidFormat(string field) =>
            Error.Validation("Validation.InvalidFormat", $"{field} has invalid format");

        public static Error TooLong(string field, int maxLength) =>
            Error.Validation("Validation.TooLong", $"{field} must not exceed {maxLength} characters");

        public static Error TooShort(string field, int minLength) =>
            Error.Validation("Validation.TooShort", $"{field} must be at least {minLength} characters");
    }

    public static class NotFound
    {
        public static Error ById(string entity, object id) =>
            Error.NotFound("NotFound.ById", $"{entity} with id '{id}' was not found");

        public static Error ByProperty(string entity, string property, object value) =>
            Error.NotFound("NotFound.ByProperty", $"{entity} with {property} '{value}' was not found");

        public static Error Cookies(string? detail = null) =>
            Error.NotFound("NotFound.Cookies", detail ?? $"cookieS was not found");   
    }

    public static class Conflict
    {
        public static Error AlreadyExists(string entity, string property, object value) =>
            Error.Conflict("Conflict.AlreadyExists", $"{entity} with {property} '{value}' already exists");

        public static Error ConcurrencyConflict(string entity) =>
            Error.Conflict("Conflict.Concurrency", $"{entity} was modified by another process");
    }

    public static class Authentication
    {
        public static Error InvalidCredentials() =>
            Error.Unauthorized("Auth.InvalidCredentials", "Invalid credentials provided");

        public static Error TokenExpired() =>
            Error.Unauthorized("Auth.TokenExpired", "Authentication token has expired");

        public static Error InsufficientPermissions() =>
            Error.Forbidden("Auth.InsufficientPermissions", "Insufficient permissions for this operation");
    }
}
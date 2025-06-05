using Common.Ddd.Domain.Values;

using Microsoft.AspNetCore.Mvc.Formatters;

namespace Common.Extensions;

/// <summary>
/// Extension methods for Result types
/// </summary>
public static class ResultExtensions
{
    public static Result<TInput> ToResult<TInput>(this TInput value) => Result<TInput>.Success(value);
    
    public static async Task<Result<TOut>> BindAsync<T, TOut>(
        this Task<Result<T>> taskResult,
        Func<T, Task<Result<TOut>>> binder)
    {
        var result = await taskResult;
        return await result.BindAsync(binder);
    }

    public static async Task<Result<TOut>> Bind<T, TOut>(
        this Task<Result<T>> taskResult,
        Func<T, Result<TOut>> binder)
    {
        var result = await taskResult;
        return result.Bind(binder);
    }

    public static async Task<Result<TOut>> MapAsync<T, TOut>(
        this Task<Result<T>> taskResult,
        Func<T, Task<TOut>> mapper)
    {
        var result = await taskResult;
        return await result.MapAsync(mapper);
    }

    public static async Task<Result<TOut>> Map<T, TOut>(
        this Task<Result<T>> taskResult,
        Func<T, TOut> mapper)
    {
        var result = await taskResult;
        return result.Map(mapper);
    }

    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> taskResult,
        Func<T, Task> action)
    {
        var result = await taskResult;
        return await result.TapAsync(action);
    }

    public static async Task<Result<T>> Tap<T>(
        this Task<Result<T>> taskResult,
        Action<T> action)
    {
        var result = await taskResult;
        return result.Tap(action);
    }

    public static async Task<Result<T>> TapErrorAsync<T>(
        this Task<Result<T>> taskResult,
        Func<ErrorList, Task> action)
    {
        var result = await taskResult;
        return await result.TapErrorAsync(action);
    }

    public static async Task<Result<T>> TapError<T>(
        this Task<Result<T>> taskResult,
        Action<ErrorList> action)
    {
        var result = await taskResult;
        return result.TapError(action);
    }

    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> taskResult,
        Func<T, Task<bool>> predicate,
        Error error)
    {
        var result = await taskResult;
        return await result.EnsureAsync(predicate, error);
    }
    
    public static async Task<Result<T>> Ensure<T>(
        this Task<Result<T>> taskResult,
        Func<T, bool> predicate,
        Error error)
    {
        var result = await taskResult;
        return result.Ensure(predicate, error);
    }
}

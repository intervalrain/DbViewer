using Microsoft.AspNetCore.Mvc.Formatters;

namespace Common.Ddd.Domain.Values;

public readonly struct Result
{
    private readonly ErrorList _errors = [];

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ErrorList Errors => IsFailure ? _errors : throw new InvalidOperationException("Cannot access errors of a successful result");
    public Error FirstError => Errors.First();
    public string ErrorMessage => IsFailure ? string.Join("; ", _errors.Select(e => e.Description)) : string.Empty;

    private Result(ErrorList errors)
    {
        _errors = errors;
        IsSuccess = !errors.Any();
    }

    private Result(Error error)
    {
        _errors = new ErrorList { error };
        IsSuccess = false;
    }

    public static Result Success() => new ErrorList();
    public static Result Failure(Error error) => new(error);
    public static Result Failure(ErrorList errors) => new(errors);
    public static Result Failure(params Error[] errors) => new(new ErrorList(errors));

    // Implicit conversions
    public static implicit operator Result(Error error) => new(error);
    public static implicit operator Result(ErrorList errors) => new(errors);
    public static implicit operator Result(Error[] errors) => new(new ErrorList(errors));

    public Result<T> Map<T>(Func<T> mapper)
    {
        return IsSuccess ? Result<T>.Success(mapper()) : Result<T>.Failure(_errors);
    }

    public async Task<Result<T>> MapAsync<T>(Func<Task<T>> mapper)
    {
        return IsSuccess ? Result<T>.Success(await mapper()) : Result<T>.Failure(_errors);
    }

    public Result Bind(Func<Result> binder)
    {
        return IsSuccess ? binder() : this;
    }

    public async Task<Result> BindAsync(Func<Task<Result>> binder)
    {
        return IsSuccess ? await binder() : this;
    }

    public void Switch(Action onSuccess, Action<ErrorList> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(new ErrorList(_errors));
    }

    public override string ToString()
    {
        return IsSuccess ? "Success" : $"Failure({string.Join(", ", _errors.Select(e => e.Code))})";
    }
}

public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly ErrorList _errors = [];

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value of a failed result.");
    public ErrorList Errors => IsFailure ? _errors : throw new InvalidOperationException("Cannot access errors of a successful result.");
    public Error FirstError => Errors.First();
    public string ErrorMessage => IsFailure ? string.Join("; ", _errors.Select(e => e.Description)) : string.Empty;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(ErrorList errors)
    {
        _value = default;
        _errors = errors;
        IsSuccess = false;
    }

    private Result(Error error)
    {
        _value = default;
        _errors = [error];
        IsSuccess = false;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
    public static implicit operator Result<T>(ErrorList errors) => new(errors);
    public static implicit operator Result<T>(Error[] errors) => new([.. errors]);

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
    public static Result<T> Failure(ErrorList errors) => new(errors);
    public static Result<T> Failure(params Error[] errors) => new([.. errors]);

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        return IsSuccess ? Result<TOut>.Success(mapper(_value!)) : Result<TOut>.Failure(_errors);
    }

    public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> mapper)
    {
        return IsSuccess ? Result<TOut>.Success(await mapper(_value!)) : Result<TOut>.Failure(_errors);
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
    {
        return IsSuccess ? binder(_value!) : Result<TOut>.Failure(_errors);
    }

    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> binder)
    {
        return IsSuccess ? await binder(_value!) : Result<TOut>.Failure(_errors);
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<ErrorList, TOut> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(_errors);
    }

    public async Task<TOut> MatchAsync<TOut>(Func<T, Task<TOut>> onSuccess, Func<ErrorList, Task<TOut>> onFailure)
    {
        return IsSuccess ? await onSuccess(_value!) : await onFailure(_errors);
    }

    public void Switch(Action<T> onSuccess, Action<ErrorList> onFailure)
    {
        if (IsSuccess)
            onSuccess(_value!);
        else
            onFailure(_errors);
    }

    public T UnwrapOr(T defaultValue) => IsSuccess ? _value! : defaultValue;
    public T UnwrapOr(Func<T> defaultValueFactory) => IsSuccess ? _value! : defaultValueFactory();

    public static Result<List<T>> Combine(params Result<T>[] results)
    {
        var errors = new ErrorList();
        var values = new List<T>();

        foreach (var result in results)
        {
            if (result.IsSuccess)
                values.Add(result.Value);
            else
                errors.AddRange(result.Errors);
        }

        return errors.Any() ? Result<List<T>>.Failure(new ErrorList(errors)) : Result<List<T>>.Success(values);
    }

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        if (IsFailure)
            return this;
        return predicate(_value!) ? this : Result<T>.Failure(error);
    }

    public async Task<Result<T>> EnsureAsync(Func<T, Task<bool>> predicate, Error error)
    {
        if (IsFailure)
            return this;
        return await predicate(_value!) ? this : Result<T>.Failure(error);
    }

    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(_value!);
        return this;
    }

    public async Task<Result<T>> TapAsync(Func<T, Task> action)
    {
        if (IsSuccess)
            await action(_value!);
        return this;
    }

    public Result<T> TapError(Action<ErrorList> action)
    {
        if (IsFailure)
            action(_errors);
        return this;
    }

    public async Task<Result<T>> TapErrorAsync(Func<ErrorList, Task> action)
    {
        if (IsFailure)
            await action(_errors);
        return this;
    }

    public override string ToString()
    {
        return IsSuccess ? $"Success({_value})" : $"Failure({string.Join(", ", _errors.Select(e => e.Code))})";
    }
}

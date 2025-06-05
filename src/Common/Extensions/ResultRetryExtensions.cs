using Common.Ddd.Domain.Values;

namespace Common.Extensions;

public static class ResultRetryExtensions
{
    public static async Task<Result<T>> RetryAsync<T>(
        this Func<Task<Result<T>>> operation,
        int maxRetries,
        TimeSpan delay)
    {
        for (int i = 0; i <= maxRetries; i++)
        {
            var result = await operation();
            if (result.IsSuccess || i == maxRetries)
                return result;

            await Task.Delay(delay);
        }

        return Error.Failure("Retry.MaxAttemptsReached", "Reach max attempts of tries");
    }
}
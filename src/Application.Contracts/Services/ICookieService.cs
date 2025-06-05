using Common.Ddd.Domain.Values;

namespace Application.Contracts.Services;

public interface ICookieService
{
    Task<Result<string>> GetAsync(string name);
    Task<Result> SetAsync(string name, string value, TimeSpan? expiry = null);
    Task<Result> RemoveAsync(string name);
}
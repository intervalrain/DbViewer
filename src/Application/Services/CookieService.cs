using Microsoft.JSInterop;
using Application.Contracts.Services;
using Common.Ddd.Domain.Values;

namespace Application.Services;

public class CookieService : ICookieService
{
    private readonly IJSRuntime _jsRuntime;

    public CookieService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<Result<string>> GetAsync(string name)
    {
        try
        {
            // 檢查 JavaScript 是否就緒
            var isReady = await _jsRuntime.InvokeAsync<bool>("isJavaScriptReady");
            if (!isReady)
            {
                Console.WriteLine("JavaScript 尚未完全載入，等待...");
                await Task.Delay(100);
                
                // 再次檢查
                isReady = await _jsRuntime.InvokeAsync<bool>("isJavaScriptReady");
                if (!isReady)
                {
                    return Result<string>.Failure(Errors.NotFound.Cookies("JavaScript 尚未就緒"));
                }
            }
            
            var value = await _jsRuntime.InvokeAsync<string>("getCookie", name);
            return string.IsNullOrEmpty(value) 
                ? Result<string>.Failure(Errors.NotFound.Cookies())
                : Result<string>.Success(value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CookieService.GetAsync 錯誤: {ex.Message}");
            return Result<string>.Failure(Errors.NotFound.Cookies(ex.Message));
        }
    }

    public async Task<Result> SetAsync(string name, string value, TimeSpan? expiry = null)
    {
        try
        {
            var days = expiry?.TotalDays ?? 7; // 預設 7 天
            await _jsRuntime.InvokeVoidAsync("setCookie", name, value, days);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Errors.NotFound.Cookies(ex.Message));
        }
    }

    public async Task<Result> RemoveAsync(string name)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("removeCookie", name);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Errors.NotFound.Cookies(ex.Message));
        }
    }
} 
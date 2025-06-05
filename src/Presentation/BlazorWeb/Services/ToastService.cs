using Microsoft.JSInterop;

namespace Presentation.BlazorWeb.Services;

public interface IToastService
{
    Task ShowError(string message, int duration = 5000);
    Task ShowSuccess(string message, int duration = 3000);
    Task ShowWarning(string message, int duration = 4000);
    Task ShowInfo(string message, int duration = 3000);
}

public class ToastService : IToastService
{
    private readonly IJSRuntime _jsRuntime;

    public ToastService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ShowError(string message, int duration = 5000)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("showToast", message, "error", duration);
        }
        catch
        {
            // 忽略 JavaScript 調用錯誤
        }
    }

    public async Task ShowSuccess(string message, int duration = 3000)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("showToast", message, "success", duration);
        }
        catch
        {
            // 忽略 JavaScript 調用錯誤
        }
    }

    public async Task ShowWarning(string message, int duration = 4000)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("showToast", message, "warning", duration);
        }
        catch
        {
            // 忽略 JavaScript 調用錯誤
        }
    }

    public async Task ShowInfo(string message, int duration = 3000)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("showToast", message, "info", duration);
        }
        catch
        {
            // 忽略 JavaScript 調用錯誤
        }
    }
} 
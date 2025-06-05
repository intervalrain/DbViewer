using Microsoft.JSInterop;
using System.Text.Json;

namespace Presentation.BlazorWeb.Services;

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
    Task<List<ConnectionRecord>> GetConnectionsAsync();
    Task SaveConnectionAsync(ConnectionRecord connection);
    Task<AppSettings?> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private const string ConnectionsKey = "dbviewer_connections";
    private const string SettingsKey = "dbviewer_settings";

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // 忽略錯誤
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // 忽略錯誤
        }
    }

    public async Task<List<ConnectionRecord>> GetConnectionsAsync()
    {
        var connections = await GetItemAsync<List<ConnectionRecord>>(ConnectionsKey);
        return connections ?? new List<ConnectionRecord>();
    }

    public async Task SaveConnectionAsync(ConnectionRecord connection)
    {
        var connections = await GetConnectionsAsync();
        
        // 移除相同 DisplayName 的舊記錄
        connections.RemoveAll(c => c.DisplayName == connection.DisplayName);
        
        // 添加新記錄到開頭
        connections.Insert(0, connection);
        
        // 限制最多保存 20 個連線記錄
        if (connections.Count > 20)
        {
            connections = connections.Take(20).ToList();
        }
        
        await SetItemAsync(ConnectionsKey, connections);
    }

    public async Task<AppSettings?> GetSettingsAsync()
    {
        return await GetItemAsync<AppSettings>(SettingsKey);
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        await SetItemAsync(SettingsKey, settings);
    }
}

public class ConnectionRecord
{
    public string DisplayName { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DefaultDatabase { get; set; } = string.Empty;
    public bool SavePassword { get; set; }
    public DateTime LastUsed { get; set; }
}

public class AppSettings
{
    public string LastUsedConnection { get; set; } = string.Empty;
    public int CurrentTheme { get; set; } = 0;
    public int HistorySize { get; set; } = 100;
    public bool AutoSaveHistory { get; set; } = true;
    public bool ShowLineNumbers { get; set; } = true;
    public int QueryTimeout { get; set; } = 30;
} 
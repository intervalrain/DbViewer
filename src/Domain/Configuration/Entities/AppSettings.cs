using System.Text.Json;
using Common.Ddd.Domain.Entities;
using Domain.Database.Entities;

namespace Domain.Configuration.Entities;

public class AppSettings(Guid id) : Entity<Guid>(id)
{
    private const string SettingsFileName = "dbtest_settings.json";
    private const string ConnectionsFileName = "dbtest_connections.json";
    private const string HistoryFileName = "dbtest_history.json";

    public List<ConnectionInfo> SavedConnections { get; set; } = [];
    public List<QueryHistoryItem> QueryHistory { get; set; } = [];
    public string LastUsedConnection { get; set; } = string.Empty;
    public int CurrentTheme { get; set; } = 0;
    public int HistorySize { get; set; } = 100;
    public bool AutoSaveHistory { get; set; } = true;
    public bool ShowLineNumbers { get; set; } = true;
    public int QueryTimeout { get; set; } = 30;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFileName))
            {
                var json = File.ReadAllText(SettingsFileName);
                var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings(Guid.NewGuid());
                settings.LoadConnections();
                settings.LoadQueryHistory();
                return settings;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"載入設定時發生錯誤: {ex.Message}");
        }

        return new AppSettings(Guid.NewGuid());
    }
    
    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var settingsToSave = new
            {
                LastUsedConnection,
                CurrentTheme,
                HistorySize,
                AutoSaveHistory,
                ShowLineNumbers,
                QueryTimeout
            };
            
            var json = JsonSerializer.Serialize(settingsToSave, options);
            File.WriteAllText(SettingsFileName, json);
            
            SaveConnections();
            SaveQueryHistory();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"儲存設定時發生錯誤: {ex.Message}");
        }
    }
    
    private void LoadConnections()
    {
        try
        {
            if (File.Exists(ConnectionsFileName))
            {
                var json = File.ReadAllText(ConnectionsFileName);
                SavedConnections = JsonSerializer.Deserialize<List<ConnectionInfo>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"載入連線資訊時發生錯誤: {ex.Message}");
            SavedConnections = new();
        }
    }
    
    private void SaveConnections()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var connectionsToSave = SavedConnections.Select(conn => 
                conn.SavePassword ? conn : conn.CloneWithoutPassword()).ToList();
            
            var json = JsonSerializer.Serialize(connectionsToSave, options);
            File.WriteAllText(ConnectionsFileName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"儲存連線資訊時發生錯誤: {ex.Message}");
        }
    }

    public void AddOrUpdateConnection(ConnectionInfo connection)
    {
        var existing = SavedConnections.FirstOrDefault(c =>
            c.Host == connection.Host &&
            c.Port == connection.Port &&
            c.Username == connection.Username);

        if (existing != null)
        {
            existing.DisplayName = connection.DisplayName;
            existing.Password = connection.Password;
            existing.DefaultDatabase = connection.DefaultDatabase;
            existing.SavePassword = connection.SavePassword;
            existing.LastUsed = DateTime.Now;
        }
        else
        {
            connection.LastUsed = DateTime.Now;
            SavedConnections.Add(connection);
        }

        SavedConnections = SavedConnections.OrderByDescending(c => c.LastUsed).ToList();
    }
    
    public void RemoveConnection(ConnectionInfo connection)
    {
        SavedConnections.RemoveAll(c => 
            c.Host == connection.Host && 
            c.Port == connection.Port && 
            c.Username == connection.Username);
    }

    private void LoadQueryHistory()
    {
        try
        {
            if (File.Exists(HistoryFileName))
            {
                var json = File.ReadAllText(HistoryFileName);
                QueryHistory = JsonSerializer.Deserialize<List<QueryHistoryItem>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"載入查詢歷史時發生錯誤: {ex.Message}");
            QueryHistory = new();
        }
    }

    private void SaveQueryHistory()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(QueryHistory, options);
            File.WriteAllText(HistoryFileName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"儲存查詢歷史時發生錯誤: {ex.Message}");
        }
    }

    public void AddQueryHistory(string sqlText, string database, int? affectedRows = null, string? errorMessage = null)
    {
        if (!AutoSaveHistory) return;

        var historyItem = new QueryHistoryItem
        {
            SqlText = sqlText,
            Database = database,
            ExecutedAt = DateTime.Now,
            AffectedRows = affectedRows,
            ErrorMessage = errorMessage ?? string.Empty
        };

        QueryHistory.Insert(0, historyItem);

        // 限制歷史記錄數量
        if (QueryHistory.Count > HistorySize)
        {
            QueryHistory = QueryHistory.Take(HistorySize).ToList();
        }
    }

    public void ClearQueryHistory()
    {
        QueryHistory.Clear();
    }

    public void RemoveQueryHistory(QueryHistoryItem item)
    {
        QueryHistory.Remove(item);
    }
}

public class QueryHistoryItem
{
    public string SqlText { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public int? AffectedRows { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
} 
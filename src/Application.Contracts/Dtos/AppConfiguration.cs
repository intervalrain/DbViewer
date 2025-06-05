namespace Application.Contracts.Dtos;

public class AppConfiguration
{
    public string DefaultTheme { get; set; } = "Default";
    public int HistorySize { get; set; } = 100;
    public int DefaultTimeout { get; set; } = 30;
    public bool AutoSaveSettings { get; set; } = true;
    public string ConnectionsFilePath { get; set; } = "connections.json";
    public string HistoryFilePath { get; set; } = "dbtest_history.txt";
} 
namespace Application.Contracts.Dtos;

public class LoggingConfiguration
{
    public FileLoggingConfiguration File { get; set; } = new();
    public ConsoleLoggingConfiguration Console { get; set; } = new();
}

public class FileLoggingConfiguration
{
    public string Path { get; set; } = "logs/dbtest_{Date}.log";
    public string RollingInterval { get; set; } = "Day";
    public int RetainedFileCountLimit { get; set; } = 7;
    public string OutputTemplate { get; set; } = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}";
}

public class ConsoleLoggingConfiguration
{
    public string OutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
} 
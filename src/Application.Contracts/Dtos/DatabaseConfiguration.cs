namespace Application.Contracts.Dtos;

public class DatabaseConfiguration
{
    public int DefaultConnectionTimeout { get; set; } = 30;
    public int DefaultCommandTimeout { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
} 
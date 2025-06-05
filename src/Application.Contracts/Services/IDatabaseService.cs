using Application.Contracts.Dtos;
using Common.Ddd.Domain.Values;
using Domain.Database.Entities;

namespace Application.Contracts.Services;

public interface IDatabaseService : IDisposable
{
    bool IsConnected { get; }
    string CurrentDatabase { get; }
    ConnectionInfoDto? CurrentConnectionInfo { get; }

    Task<Result> ConnectAsync(ConnectionInfoDto connectionInfo, string? database = null);
    Task<Result> DisconnectAsync();
    Task<Result> TestConnectionAsync(ConnectionInfoDto connectionInfo);
    Task<Result<List<DatabaseInfo>>> GetDatabasesAsync();
    Task<Result> SwitchDatabaseAsync(string databaseName);
    Task<Result<QueryResultDto>> ExecuteQueryAsync(string sql, int timeoutSeconds = 30);
    Task<Result<int>> ExecuteNonQueryAsync(string sql, int timeoutSeconds = 30);
    Task<Result<List<string>>> GetTablesAsync();
    Result<bool> IsSelectQuery(string sql);
} 
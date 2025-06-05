using Microsoft.Extensions.Logging;
using Application.Contracts.Services;
using Domain.Database.Entities;
using Application.Contracts.Dtos;
using Common.Ddd.Domain.Values;

namespace Application.Services;

public class DatabaseApplicationService : IApplicationService
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<DatabaseApplicationService> _logger;

    public DatabaseApplicationService(
        IDatabaseService databaseService,
        ILogger<DatabaseApplicationService> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("DatabaseApplicationService 已啟動");
        await Task.CompletedTask;
    }

    public async Task<Result<bool>> ConnectToDatabaseAsync(ConnectionInfoDto connectionInfo, string? database = null)
    {
        _logger.LogInformation("嘗試連接到資料庫 {Database} 於 {Host}:{Port}",
                database ?? connectionInfo.DefaultDatabase, connectionInfo.Host, connectionInfo.Port);

        var result = await _databaseService.ConnectAsync(connectionInfo, database);
        return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(Errors.Database.ConnectionFailed(result.ErrorMessage));
    }

    public async Task<Result<bool>> TestConnectionAsync(ConnectionInfoDto connectionInfo)
    {
        var result = await _databaseService.TestConnectionAsync(connectionInfo);
        return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(Errors.Database.ConnectionFailed(result.ErrorMessage));
    }

    public async Task<Result<List<DatabaseInfo>>> GetDatabasesAsync()
    {
        var result = await _databaseService.GetDatabasesAsync();
        return result.IsSuccess ? 
            Result<List<DatabaseInfo>>.Success(result.Value) : 
            Result<List<DatabaseInfo>>.Failure(Errors.Database.ConnectionFailed(result.ErrorMessage));
    }

    public async Task<Result<QueryResultDto>> ExecuteQueryAsync(string sql, int timeoutSeconds = 30)
    {
        var result = await _databaseService.ExecuteQueryAsync(sql, timeoutSeconds);
        return result.IsSuccess ? 
            Result<QueryResultDto>.Success(result.Value) : 
            Result<QueryResultDto>.Failure(Errors.Database.ConnectionFailed(result.ErrorMessage));
    }

    public async Task<Result<int>> ExecuteNonQueryAsync(string sql, int timeoutSeconds = 30)
    {
        var result = await _databaseService.ExecuteNonQueryAsync(sql, timeoutSeconds);
        return result.IsSuccess ? 
            Result<int>.Success(result.Value) : 
            Result<int>.Failure(Errors.Database.ConnectionFailed(result.ErrorMessage));
    }

    public Result<bool> IsConnected => _databaseService.IsConnected;
    public Result<string> CurrentDatabase => _databaseService.CurrentDatabase;
    public Result<ConnectionInfoDto?> CurrentConnectionInfo => _databaseService.CurrentConnectionInfo;

    public async Task<Result<bool>> DisconnectAsync()
    {
        var result = await _databaseService.DisconnectAsync();
        return result.IsSuccess ? 
            Result<bool>.Success(true) : 
            Result<bool>.Failure(Errors.Database.ConnectionFailed(result.ErrorMessage));
    }
} 
using System.Data;
using Npgsql;
using Microsoft.Extensions.Logging;
using Domain.Database.Entities;
using Application.Contracts.Services;
using Application.Contracts.Dtos;
using AutoMapper;
using Common.Ddd.Domain.Values;

namespace Infrastructure.Data;

public class PostgresDatabaseService : IDatabaseService
{
    private NpgsqlConnection? _connection;
    private readonly ILogger<PostgresDatabaseService> _logger;
    private readonly IMapper _mapper;
    private string _currentDatabase = "未連接";
    private ConnectionInfoDto? _currentConnectionInfo;
    
    public bool IsConnected => _connection?.State == ConnectionState.Open;
    public string CurrentDatabase => _currentDatabase;
    public ConnectionInfoDto? CurrentConnectionInfo => _currentConnectionInfo;
    
    public PostgresDatabaseService(ILogger<PostgresDatabaseService> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }
    
    public async Task<Result> ConnectAsync(ConnectionInfoDto input, string? database = "postgres")
    {
        try
        {
            await DisconnectAsync();
            
            var connectionInfo = _mapper.Map<ConnectionInfo>(input);
            var dbName = database ?? connectionInfo.DefaultDatabase;
            var connectionString = connectionInfo.BuildConnectionString(dbName);
            
            _connection = new NpgsqlConnection(connectionString);
            await _connection.OpenAsync();
            
            _currentDatabase = dbName;
            _currentConnectionInfo = input;
            
            _logger.LogInformation("成功連接到資料庫 {Database} 於 {Host}:{Port}", 
                dbName, connectionInfo.Host, connectionInfo.Port);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "連接資料庫失敗: {Message}", ex.Message);
            return Result.Failure(Errors.Database.ConnectionFailed(ex.Message));
        }
    }

    public async Task<Result> DisconnectAsync()
    {
        if (_connection != null)
        {
            try
            {
                if (_connection.State == ConnectionState.Open)
                {
                    await _connection.CloseAsync();
                    _logger.LogInformation("已斷開資料庫連接");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "斷開連接時發生錯誤: {Message}", ex.Message);
                return Result.Failure(Errors.Database.ConnectionFailed(ex.Message));
            }
            finally
            {
                await _connection.DisposeAsync();
                _connection = null;
                _currentDatabase = "未連接";
                _currentConnectionInfo = null;
            }
        }
        return Result.Success();
    }
    
    public async Task<Result> TestConnectionAsync(ConnectionInfoDto input)
    {
        try
        {
            var connectionInfo = _mapper.Map<ConnectionInfo>(input);
            var connectionString = connectionInfo.BuildConnectionString();
            using var testConnection = new NpgsqlConnection(connectionString);
            await testConnection.OpenAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "測試連接失敗: {Message}", ex.Message);
            return Result.Failure(Errors.Database.ConnectionFailed(ex.Message));
        }
    }
    public async Task<Result<List<DatabaseInfo>>> GetDatabasesAsync()
    {
        if (!IsConnected)
            return new List<DatabaseInfo>();
        
        try
        {
            var databases = new List<DatabaseInfo>();
            var sql = @"
                    SELECT 
                        d.datname as name,
                        pg_catalog.pg_get_userbyid(d.datdba) as owner,
                        pg_catalog.pg_encoding_to_char(d.encoding) as encoding,
                        d.datcollate as collate,
                        d.datctype as ctype,
                        CASE WHEN pg_catalog.has_database_privilege(d.datname, 'CONNECT')
                             THEN pg_catalog.pg_database_size(d.datname)
                             ELSE 0 END as size,
                        shobj.description
                    FROM pg_catalog.pg_database d
                    LEFT JOIN pg_catalog.pg_shdescription shobj ON d.oid = shobj.objoid
                    WHERE d.datistemplate = false
                    ORDER BY d.datname;";
            
            using var command = new NpgsqlCommand(sql, _connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                databases.Add(new DatabaseInfo(Guid.NewGuid())
                {
                    Name = reader.GetString("name"),
                    Owner = reader.GetString("owner"),
                    Encoding = reader.GetString("encoding"),
                    Collate = reader.GetString("collate"),
                    CType = reader.GetString("ctype"),
                    Size = reader.GetInt64("size"),
                    Description = reader.IsDBNull("description") ? "" : reader.GetString("description")
                });
            }
            
            return Result<List<DatabaseInfo>>.Success(databases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取資料庫列表失敗: {Message}", ex.Message);
            return Result<List<DatabaseInfo>>.Failure(Errors.Database.ConnectionFailed(ex.Message));
        }
    }
    
    public async Task<Result> SwitchDatabaseAsync(string databaseName)
    {
        if (_currentConnectionInfo == null)
            return Result.Failure(Errors.Database.ConnectionFailed());
        
        var result = await ConnectAsync(_currentConnectionInfo, databaseName);
        return result.IsSuccess ? Result.Success() : Result.Failure(Errors.Database.ConnectionFailed(result.ErrorMessage));
    }
    
    public async Task<Result<QueryResultDto>> ExecuteQueryAsync(string sql, int timeoutSeconds = 30)
    {
        if (!IsConnected)
            return Result<QueryResultDto>.Failure(Errors.Database.ConnectionFailed());
        
        try
        {
            using var command = new NpgsqlCommand(sql, _connection);
            command.CommandTimeout = timeoutSeconds;
            
            var dataTable = new DataTable();
            using var reader = await command.ExecuteReaderAsync();
            
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }
            
            while (await reader.ReadAsync())
            {
                var row = dataTable.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }
                dataTable.Rows.Add(row);
            }
            
            _logger.LogInformation("查詢執行成功，返回 {RowCount} 行", dataTable.Rows.Count);
            return Result<QueryResultDto>.Success(new QueryResultDto
            {
                Data = dataTable,
                AffectedRows = 0,
                ExecutionTimeMs = 0,
                IsSelectQuery = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行查詢失敗: {Message}", ex.Message);
            return Result<QueryResultDto>.Failure(Errors.Database.ConnectionFailed(ex.Message));
        }
    }
    
    public async Task<Result<int>> ExecuteNonQueryAsync(string sql, int timeoutSeconds = 30)
    {
        if (!IsConnected)
            return Result<int>.Failure(Errors.Database.ConnectionFailed());
        
        try
        {
            using var command = new NpgsqlCommand(sql, _connection);
            command.CommandTimeout = timeoutSeconds;
            
            var affectedRows = await command.ExecuteNonQueryAsync();
            _logger.LogInformation("命令執行成功，影響 {AffectedRows} 行", affectedRows);
            return Result<int>.Success(affectedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行命令失敗: {Message}", ex.Message);
            return Result<int>.Failure(Errors.Database.ConnectionFailed(ex.Message));
        }
    }

    public async Task<Result<List<string>>> GetTablesAsync()
    {
        if (!IsConnected)
            return Result<List<string>>.Failure(Errors.Database.ConnectionFailed());

        try
        {
            var tables = new List<string>();
            var sql = @"
                    SELECT table_name 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_type = 'BASE TABLE'
                    ORDER BY table_name;";

            using var command = new NpgsqlCommand(sql, _connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return Result<List<string>>.Success(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取表格列表失敗: {Message}", ex.Message);
            return Result<List<string>>.Failure(Errors.Database.ConnectionFailed(ex.Message));
        }
    }
    
    public Result<bool> IsSelectQuery(string sql)
    {
        var trimmedSql = sql.Trim();
        var result = 
            trimmedSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
            trimmedSql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
            trimmedSql.StartsWith("SHOW", StringComparison.OrdinalIgnoreCase) ||
            trimmedSql.StartsWith("EXPLAIN", StringComparison.OrdinalIgnoreCase) ||
            trimmedSql.StartsWith("DESCRIBE", StringComparison.OrdinalIgnoreCase);
        return result ? Result<bool>.Success(true) : Result<bool>.Failure(Errors.Database.ConnectionFailed());
    }
    
    public void Dispose()
    {
        DisconnectAsync().Wait();
    }
}

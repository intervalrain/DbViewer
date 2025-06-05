using Microsoft.AspNetCore.Mvc;
using Application.Contracts.Services;
using System.Data;
using Application.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Presentation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DatabaseController : ApiController
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(ILogger<DatabaseController> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    [HttpPost("connect")]
    public async Task<IActionResult> Connect([FromBody] ConnectionInfoDto connectionInfo, [FromQuery] string? database = null)
    {
        try
        {
            var result = await _databaseService.ConnectAsync(connectionInfo, database);
            if (result.IsSuccess)
            {
                return Ok(new { success = true, message = "連線成功", database = _databaseService.CurrentDatabase });
            }
            return BadRequest(new { success = false, message = "連線失敗" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "連線資料庫時發生錯誤");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect()
    {
        try
        {
            await _databaseService.DisconnectAsync();
            return Ok(new { success = true, message = "已中斷連線" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "中斷連線時發生錯誤");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("test-connection")]
    public async Task<IActionResult> TestConnection([FromBody] ConnectionInfoDto connectionInfo)
    {
        try
        {
            var result = await _databaseService.TestConnectionAsync(connectionInfo);
            return Ok(new { success = result.IsSuccess, message = result.IsSuccess ? "連線測試成功" : "連線測試失敗" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "測試連線時發生錯誤");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new 
        { 
            isConnected = _databaseService.IsConnected,
            currentDatabase = _databaseService.CurrentDatabase,
            connectionInfo = _databaseService.CurrentConnectionInfo
        });
    }

    [HttpGet("databases")]
    public async Task<IActionResult> GetDatabases()
    {
        try
        {
            if (!_databaseService.IsConnected)
            {
                return BadRequest(new { success = false, message = "尚未連線到資料庫" });
            }

            var databases = await _databaseService.GetDatabasesAsync();
            return Ok(new { success = true, databases });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得資料庫清單時發生錯誤");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("switch-database")]
    public async Task<IActionResult> SwitchDatabase([FromQuery] string databaseName)
    {
        try
        {
            if (!_databaseService.IsConnected)
            {
                return BadRequest(new { success = false, message = "尚未連線到資料庫" });
            }

            var result = await _databaseService.SwitchDatabaseAsync(databaseName);
            if (result.IsSuccess)
            {
                return Ok(new { success = true, message = $"已切換到資料庫: {databaseName}", currentDatabase = _databaseService.CurrentDatabase });
            }
            return BadRequest(new { success = false, message = "切換資料庫失敗" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切換資料庫時發生錯誤");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("tables")]
    public async Task<IActionResult> GetTables()
    {
        try
        {
            if (!_databaseService.IsConnected)
            {
                return BadRequest(new { success = false, message = "尚未連線到資料庫" });
            }

            var tables = await _databaseService.GetTablesAsync();
            return Ok(new { success = true, tables });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得資料表清單時發生錯誤");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("execute-query")]
    public async Task<IActionResult> ExecuteQuery([FromBody] QueryRequest request)
    {
        try
        {
            if (!_databaseService.IsConnected)
                {
                    return BadRequest(new { success = false, message = "尚未連線到資料庫" });
                }

            if (request.Sql.IsNullOrWhiteSpace())
            {
                return BadRequest(new { success = false, message = "SQL 查詢不能為空" });
            }

            if (_databaseService.IsSelectQuery(request.Sql).IsSuccess)
            {
                var dataTable = await _databaseService.ExecuteQueryAsync(request.Sql, request.TimeoutSeconds);
                var result = ConvertDataTableToJson(dataTable.Value.Data!);
                return Ok(new { success = true, data = result, rowCount = dataTable.Value.Data!.Rows.Count });
            }
            else
            {
                var affectedRows = await _databaseService.ExecuteNonQueryAsync(request.Sql, request.TimeoutSeconds);
                return Ok(new { success = true, affectedRows, message = $"執行完成，影響 {affectedRows} 筆資料" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行查詢時發生錯誤: {Sql}", request.Sql);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private static object ConvertDataTableToJson(DataTable dataTable)
    {
        var columns = dataTable.Columns.Cast<DataColumn>()
            .Select(column => new { name = column.ColumnName, type = column.DataType.Name })
            .ToArray();

        var rows = dataTable.AsEnumerable()
            .Select(row => dataTable.Columns.Cast<DataColumn>()
                .ToDictionary(column => column.ColumnName, column => row[column] == DBNull.Value ? null : row[column]))
            .ToArray();

        return new { columns, rows };
    }
}

public class QueryRequest
{
    public string Sql { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
} 
using System.Data;

namespace Application.Contracts.Dtos;

/// <summary>
/// 資料庫查詢結果
/// </summary>
public class QueryResultDto
{
    /// <summary>
    /// 查詢結果資料表
    /// </summary>
    public DataTable? Data { get; set; }

    /// <summary>
    /// 受影響的行數（用於 INSERT、UPDATE、DELETE 等操作）
    /// </summary>
    public int AffectedRows { get; set; }

    /// <summary>
    /// 查詢執行時間（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// 是否為 SELECT 查詢
    /// </summary>
    public bool IsSelectQuery { get; set; }
} 
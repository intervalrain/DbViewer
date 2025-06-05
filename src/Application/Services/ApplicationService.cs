using Application.Base;
using Application.Contracts.Dtos;
using Application.Contracts.Services;
using AutoMapper;
using Domain.Configuration;
using Domain.Database.Entities;
using Domain.History;
using Domain.Menu;
using Domain.Shared.Themes;
using Domain.Themes;
using Figgle;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ApplicationService : ApplicationServiceBase
{
    public ApplicationService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public async override Task RunAsync()
    {
        try
        {
            DisplayWelcomeMessage();

            await RunMainFlowAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程式執行時發生錯誤: {ex.Message}");
            _logger.LogError(ex, "程式執行時發生錯誤");
        }
        finally
        {
            await CleanupAsync();
        }
    }

    private void DisplayWelcomeMessage()
    {
        Console.Clear();

        var title = FiggleFonts.Standard.Render("PostgreSQL Tool");
        _themeManager.WriteLine(title, ThemeStyle.Heading);

        _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
        _themeManager.WriteLine("║                PostgreSQL 資料庫管理工具 v2.0                ║", ThemeStyle.Heading);
        _themeManager.WriteLine("║                                                              ║", ThemeStyle.Border);
        _themeManager.WriteLine("║  功能特色:                                                   ║", ThemeStyle.Info);
        _themeManager.WriteLine("║  • 常用連線管理                                             ║", ThemeStyle.Info);
        _themeManager.WriteLine("║  • 資料庫切換                                               ║", ThemeStyle.Info);
        _themeManager.WriteLine("║  • 多主題支援                                               ║", ThemeStyle.Info);
        _themeManager.WriteLine("║  • 命令歷史記錄                                             ║", ThemeStyle.Info);
        _themeManager.WriteLine("║  • 智能自動完成                                             ║", ThemeStyle.Info);
        _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);

        Console.WriteLine();
        _themeManager.Write("目前主題: ", ThemeStyle.Label);
        _themeManager.WriteLine(_themeManager.CurrentThemeName, ThemeStyle.Highlight);

        Console.WriteLine();
        _themeManager.WriteLine("按任意鍵繼續...", ThemeStyle.Info);
        PressToContinue();
    }

    private async Task RunMainFlowAsync()
    {
        while (true)
        {
            try
            {
                var selectedConnection = _menuManager.ShowConnectionMenu(_appSettings.SavedConnections);

                if (selectedConnection == null)
                {
                    if (_menuManager.ShowConfirmDialog("確定要退出程式嗎？", "退出確認"))
                    {
                        break;
                    }
                    continue;
                }

                _themeManager.WriteLine("正在連接...", ThemeStyle.Info);
                var connection = _mapper.Map<ConnectionInfo, ConnectionInfoDto>(selectedConnection);
                var connected = await _databaseService.ConnectAsync(connection);

                if (!connected.IsSuccess)
                {
                    _themeManager.WriteLine("連接失敗！", ThemeStyle.Error);
                    _themeManager.WriteLine("按任意鍵繼續...", ThemeStyle.Info);
                    PressToContinue();
                    continue;
                }

                _appSettings.AddOrUpdateConnection(selectedConnection);
                _appSettings.LastUsedConnection = selectedConnection.DisplayName;
                _appSettings.Save();

                _themeManager.WriteLine("連接成功！", ThemeStyle.Success);

                await RunCommandLoopAsync();
            }
            catch (Exception ex)
            {
                _themeManager.WriteLine($"發生錯誤: {ex.Message}", ThemeStyle.Error);
                _logger.LogError(ex, "主流程發生錯誤");

                _themeManager.WriteLine("按任意鍵繼續...", ThemeStyle.Info);
                PressToContinue();
            }
        }
    }

    private async Task RunCommandLoopAsync()
    {
        Console.Clear();
        _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
        _themeManager.WriteLine("║                        SQL 命令介面                          ║", ThemeStyle.Heading);
        _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);

        DisplayConnectionInfo();
        DisplayHelp();

        while (_databaseService.IsConnected)
        {
            try
            {
                Console.WriteLine();
                _themeManager.Write($"{_databaseService.CurrentDatabase}", ThemeStyle.DatabaseName);
                _themeManager.Write("=# ", ThemeStyle.Prompt);

                var command = ReadLineWithHistory();

                if (string.IsNullOrWhiteSpace(command))
                    continue;

                if (await ProcessSpecialCommandAsync(command))
                    continue;

                // 新增到歷史記錄
                _historyManager.Add(command);

                // 執行 SQL 命令
                await ExecuteSQLAsync(command);
            }
            catch (Exception ex)
            {
                _themeManager.WriteLine($"執行命令時發生錯誤: {ex.Message}", ThemeStyle.Error);
                _logger.LogError(ex, "執行命令時發生錯誤");
            }
        }
    }

    private void DisplayConnectionInfo()
    {
        Console.WriteLine();
        _themeManager.Write("連線資訊: ", ThemeStyle.Label);
        if (_databaseService.CurrentConnectionInfo != null)
        {
            _themeManager.WriteLine($"{_databaseService.CurrentConnectionInfo.Host}:{_databaseService.CurrentConnectionInfo.Port}/{_databaseService.CurrentDatabase}", ThemeStyle.Info);
        }
    }

    private void DisplayHelp()
    {
        Console.WriteLine();
        _themeManager.WriteLine("可用命令:", ThemeStyle.Label);
        _themeManager.WriteLine("  \\q          - 退出", ThemeStyle.Info);
        _themeManager.WriteLine("  \\d          - 列出所有表格", ThemeStyle.Info);
        _themeManager.WriteLine("  \\db         - 切換資料庫", ThemeStyle.Info);
        _themeManager.WriteLine("  \\h          - 顯示此說明", ThemeStyle.Info);
        _themeManager.WriteLine("  \\history    - 顯示命令歷史", ThemeStyle.Info);
        _themeManager.WriteLine("  \\clear      - 清除螢幕", ThemeStyle.Info);
        _themeManager.WriteLine("  \\theme      - 切換主題", ThemeStyle.Info);
        Console.WriteLine();
        _themeManager.WriteLine("提示: 使用 ↑↓ 鍵瀏覽命令歷史，Tab 鍵自動完成", ThemeStyle.Info);
    }

    private string ReadLineWithHistory()
    {
        var input = new List<char>();
        var historyIndex = -1;
        var history = _historyManager.GetAll();
        var cursorPosition = 0;
        var promptLength = $"{_databaseService.CurrentDatabase}=# ".Length;

        while (true)
        {

            var keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return new string(input.ToArray());

                case ConsoleKey.UpArrow:
                    if (history.Count > 0)
                    {
                        historyIndex = Math.Min(historyIndex + 1, history.Count - 1);
                        ClearCurrentLine();
                        _themeManager.Write($"{_databaseService.CurrentDatabase}", ThemeStyle.DatabaseName);
                        _themeManager.Write("=# ", ThemeStyle.Prompt);

                        input.Clear();
                        input.AddRange(history[historyIndex].ToCharArray());
                        cursorPosition = input.Count;
                        Console.Write(new string(input.ToArray()));
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (historyIndex >= 0)
                    {
                        historyIndex--;
                        ClearCurrentLine();
                        _themeManager.Write($"{_databaseService.CurrentDatabase}", ThemeStyle.DatabaseName);
                        _themeManager.Write("=# ", ThemeStyle.Prompt);

                        input.Clear();
                        if (historyIndex >= 0)
                        {
                            input.AddRange(history[historyIndex].ToCharArray());
                        }
                        cursorPosition = input.Count;
                        Console.Write(new string(input.ToArray()));
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        if (Console.CursorLeft > 0)
                        {
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        }
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (cursorPosition < input.Count)
                    {
                        cursorPosition++;
                        if (Console.CursorLeft < Console.WindowWidth - 1)
                        {
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                        }
                    }
                    break;

                case ConsoleKey.Backspace:
                    if (cursorPosition > 0)
                    {
                        input.RemoveAt(cursorPosition - 1);
                        cursorPosition--;
                        RedrawInputLine(input, cursorPosition, promptLength);
                    }
                    break;

                case ConsoleKey.Delete:
                    if (cursorPosition < input.Count)
                    {
                        input.RemoveAt(cursorPosition);
                        RedrawInputLine(input, cursorPosition, promptLength);
                    }
                    break;

                case ConsoleKey.Home:
                    cursorPosition = 0;
                    Console.SetCursorPosition(promptLength, Console.CursorTop);
                    break;

                case ConsoleKey.End:
                    cursorPosition = input.Count;
                    Console.SetCursorPosition(promptLength + input.Count, Console.CursorTop);
                    break;

                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        input.Insert(cursorPosition, keyInfo.KeyChar);
                        cursorPosition++;
                        RedrawInputLine(input, cursorPosition, promptLength);
                    }
                    break;
            }
        }
    }

    private void ClearCurrentLine()
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.SetCursorPosition(0, Console.CursorTop);
    }

    private void RedrawInputLine(List<char> input, int cursorPosition, int promptLength)
    {
        // 儲存目前游標位置
        var currentTop = Console.CursorTop;

        // 移到輸入區域的開始
        Console.SetCursorPosition(promptLength, currentTop);

        // 清除舊的輸入並重新繪製
        Console.Write(new string(' ', Console.WindowWidth - promptLength));
        Console.SetCursorPosition(promptLength, currentTop);
        Console.Write(new string(input.ToArray()));

        // 設定正確的游標位置
        Console.SetCursorPosition(promptLength + cursorPosition, currentTop);
    }

    private async Task<bool> ProcessSpecialCommandAsync(string command)
    {
        if (!command.StartsWith("\\"))
            return false;

        var cmd = command.ToLower().Trim();

        switch (cmd)
        {
            case "\\q":
            case "\\quit":
            case "\\exit":
                await _databaseService.DisconnectAsync();
                return true;

            case "\\h":
            case "\\help":
                DisplayHelp();
                return true;

            case "\\clear":
                Console.Clear();
                _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
                _themeManager.WriteLine("║                        SQL 命令介面                          ║", ThemeStyle.Heading);
                _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);
                DisplayConnectionInfo();
                DisplayHelp();
                return true;

            case "\\d":
                await ListTablesAsync();
                return true;

            case "\\db":
                await SwitchDatabaseAsync();
                return true;

            case "\\history":
                DisplayHistory();
                return true;

            case "\\theme":
                _menuManager.ShowThemeMenu();
                Console.Clear();
                _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
                _themeManager.WriteLine("║                        SQL 命令介面                          ║", ThemeStyle.Heading);
                _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);
                DisplayConnectionInfo();
                DisplayHelp();
                return true;

            default:
                _themeManager.WriteLine($"未知命令: {command}", ThemeStyle.Error);
                _themeManager.WriteLine("輸入 \\h 查看可用命令", ThemeStyle.Info);
                return true;
        }
    }

    private async Task ExecuteSQLAsync(string sql)
    {
        try
        {
            _themeManager.WriteLine("正在執行 SQL...", ThemeStyle.Info);

            // 判斷是查詢還是非查詢命令
            var trimmedSql = sql.Trim().ToUpper();
            if (trimmedSql.StartsWith("SELECT") || trimmedSql.StartsWith("WITH") || trimmedSql.StartsWith("SHOW"))
            {
                // 執行查詢
                var result = await _databaseService.ExecuteQueryAsync(sql);
                if (result.IsSuccess)
                {
                    DisplayQueryResult(result.Value.Data!);
                }
                else
                {
                    _themeManager.WriteLine($"執行查詢時發生錯誤: {result.Errors.ErrorMessage}", ThemeStyle.Error);
                }
            }
            else
            {
                // 執行非查詢命令
                var affectedRows = await _databaseService.ExecuteNonQueryAsync(sql);
                _themeManager.WriteLine($"命令執行完成，影響 {affectedRows} 行", ThemeStyle.Success);
            }
        }
        catch (Exception ex)
        {
            _themeManager.WriteLine($"執行 SQL 時發生錯誤: {ex.Message}", ThemeStyle.Error);
        }
    }

    private void DisplayQueryResult(System.Data.DataTable result)
    {
        if (result.Rows.Count == 0)
        {
            _themeManager.WriteLine("查詢結果為空", ThemeStyle.Info);
            return;
        }

        // 顯示欄位標題
        var columnNames = result.Columns.Cast<System.Data.DataColumn>()
                                       .Select(column => column.ColumnName)
                                       .ToArray();

        _themeManager.WriteLine(string.Join(" | ", columnNames), ThemeStyle.Label);
        _themeManager.WriteLine(new string('-', string.Join(" | ", columnNames).Length), ThemeStyle.Border);

        // 顯示資料行
        foreach (System.Data.DataRow row in result.Rows)
        {
            var values = row.ItemArray.Select(field => field?.ToString() ?? "NULL").ToArray();
            _themeManager.WriteLine(string.Join(" | ", values), ThemeStyle.Info);
        }

        _themeManager.WriteLine($"({result.Rows.Count} 行)", ThemeStyle.Label);
    }

    private async Task CleanupAsync()
    {
        try
        {
            _historyManager.SaveToFile(_configurationService.Application.HistoryFilePath);

            await _databaseService.DisconnectAsync();

            _logger.LogInformation("應用程式清理完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理資源時發生錯誤");
        }
    }

    private async Task ListTablesAsync()
    {
        try
        {
            _themeManager.WriteLine("正在查詢表格列表...", ThemeStyle.Info);
            var result = await _databaseService.ExecuteQueryAsync("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;");

            if (result.Value.Data!.Rows.Count > 0)
            {
                _themeManager.WriteLine("資料表列表:", ThemeStyle.Label);
                foreach (System.Data.DataRow row in result.Value.Data!.Rows)
                {
                    _themeManager.WriteLine($"  {row[0]}", ThemeStyle.Info);
                }
            }
            else
            {
                _themeManager.WriteLine("沒有找到任何資料表", ThemeStyle.Info);
            }
        }
        catch (Exception ex)
        {
            _themeManager.WriteLine($"查詢表格時發生錯誤: {ex.Message}", ThemeStyle.Error);
        }
    }

    private async Task SwitchDatabaseAsync()
    {
        try
        {
            var databases = await _databaseService.GetDatabasesAsync();
            var selectedDatabase = _menuManager.ShowDatabaseMenu(databases.Value, _databaseService.CurrentDatabase);

            if (!string.IsNullOrEmpty(selectedDatabase) && selectedDatabase != _databaseService.CurrentDatabase)
            {
                _themeManager.WriteLine($"正在切換到資料庫: {selectedDatabase}", ThemeStyle.Info);
                var connected = await _databaseService.ConnectAsync(_databaseService.CurrentConnectionInfo!, selectedDatabase);

                if (connected.IsSuccess)
                {
                    _themeManager.WriteLine($"已成功切換到資料庫: {selectedDatabase}", ThemeStyle.Success);
                }
                else
                {
                    _themeManager.WriteLine("切換資料庫失敗", ThemeStyle.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _themeManager.WriteLine($"切換資料庫時發生錯誤: {ex.Message}", ThemeStyle.Error);
        }
    }

    private void DisplayHistory()
    {
        var history = _historyManager.GetAll();
        if (history.Count > 0)
        {
            _themeManager.WriteLine("命令歷史記錄:", ThemeStyle.Label);
            for (int i = 0; i < history.Count; i++)
            {
                _themeManager.WriteLine($"  {i + 1}. {history[i]}", ThemeStyle.Info);
            }
        }
        else
        {
            _themeManager.WriteLine("沒有命令歷史記錄", ThemeStyle.Info);
        }
    }

    private void PressToContinue()
    {
        Console.ReadKey(true);
    }
}

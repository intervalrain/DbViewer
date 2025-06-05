using Domain.Database.Entities;
using Domain.Shared.Themes;
using Domain.Themes;

namespace Domain.Menu;

public class MenuManager
{
    private readonly ThemeManager _themeManager;
    
    public MenuManager(ThemeManager themeManager)
    {
        _themeManager = themeManager;
    }
    
    public ConnectionInfo? ShowConnectionMenu(List<ConnectionInfo> connections)
    {
        if (!connections.Any())
        {
            Console.Clear();
            _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
            _themeManager.WriteLine("║                        歡迎使用資料庫工具                    ║", ThemeStyle.Heading);
            _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);
            Console.WriteLine();
            _themeManager.WriteLine("目前沒有已儲存的連線。", ThemeStyle.Info);
            Console.WriteLine();
            
            var initialOptions = new List<string> { "新增連線", "退出" };
            int initialSelectedIndex = ShowMenu(initialOptions, "請選擇一個選項:");
            
            if (initialSelectedIndex == 0) 
            {
                return CreateNewConnection();
            }
            else
            {
                return null;
            }
        }
        
        Console.Clear();
        _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
        _themeManager.WriteLine("║                        選擇資料庫連線                        ║", ThemeStyle.Heading);
        _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);
        Console.WriteLine();
        
        var options = new List<string>();
        options.Add("新增連線");
        options.AddRange(connections.Select(c => $"{c.DisplayName} ({c.Host}:{c.Port})"));
        options.Add("返回");
        
        int selectedIndex = ShowMenu(options, "請選擇一個選項:");
        
        if (selectedIndex == 0) 
        {
            return CreateNewConnection();
        }
        else if (selectedIndex == options.Count - 1) 
        {
            return null;
        }
        else if (selectedIndex > 0 && selectedIndex < options.Count - 1)
        {
            var connection = connections[selectedIndex - 1];
            
            if (!connection.SavePassword || string.IsNullOrEmpty(connection.Password))
            {
                _themeManager.Write("密碼: ", ThemeStyle.Label);
                connection.Password = ReadPassword();
            }
            
            return connection;
        }
        
        return null;
    }
    
    public string? ShowDatabaseMenu(List<DatabaseInfo> databases, string currentDatabase)
    {
        if (!databases.Any())
        {
            _themeManager.WriteLine("沒有可用的資料庫。", ThemeStyle.Info);
            return null;
        }
        
        Console.Clear();
        _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
        _themeManager.WriteLine("║                        選擇資料庫                            ║", ThemeStyle.Heading);
        _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);
        Console.WriteLine();
        
        _themeManager.Write("目前資料庫: ", ThemeStyle.Label);
        _themeManager.WriteLine(currentDatabase, ThemeStyle.DatabaseName);
        Console.WriteLine();
        
        var options = databases.Select(db => 
            $"{db.Name} ({db.FormattedSize}) - {db.Owner}").ToList();
        options.Add("取消");
        
        int selectedIndex = ShowMenu(options, "請選擇資料庫:");
        
        if (selectedIndex >= 0 && selectedIndex < databases.Count)
        {
            return databases[selectedIndex].Name;
        }
        
        return null;
    }
    
    public void ShowThemeMenu()
    {
        var themes = _themeManager.GetAvailableThemes();
        
        Console.Clear();
        _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
        _themeManager.WriteLine("║                        選擇主題                              ║", ThemeStyle.Heading);
        _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);
        Console.WriteLine();
        
        _themeManager.Write("目前主題: ", ThemeStyle.Label);
        _themeManager.WriteLine(_themeManager.CurrentThemeName, ThemeStyle.Highlight);
        Console.WriteLine();
        
        var options = themes.ToList();
        options.Add("取消");
        
        int selectedIndex = ShowMenu(options, "請選擇主題:");
        
        if (selectedIndex >= 0 && selectedIndex < themes.Count)
        {
            _themeManager.SetTheme(selectedIndex);
            _themeManager.WriteLine($"已切換到 {themes[selectedIndex]} 主題", ThemeStyle.Success);
            Thread.Sleep(1000);
        }
    }
    
    private int ShowMenu(List<string> options, string prompt)
    {
        int selectedIndex = 0;
        ConsoleKeyInfo keyInfo;
        
        // 記錄初始游標位置
        int startTop = Console.CursorTop;

        do
        {
            // 回到初始位置
            Console.SetCursorPosition(0, startTop);

            // 清除之前的內容
            for (int i = 0; i < options.Count + 3; i++)
            {
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.WriteLine();
            }

            // 回到初始位置重新繪製
            Console.SetCursorPosition(0, startTop);

            _themeManager.WriteLine(prompt, ThemeStyle.Label);
            Console.WriteLine();

            for (int i = 0; i < options.Count; i++)
            {
                if (i == selectedIndex)
                {
                    _themeManager.Write("► ", ThemeStyle.MenuSelected);
                    _themeManager.WriteLine(options[i], ThemeStyle.MenuSelected);
                }
                else
                {
                    _themeManager.Write("  ", ThemeStyle.MenuOption);
                    _themeManager.WriteLine(options[i], ThemeStyle.MenuOption);
                }
            }

            keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + options.Count) % options.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % options.Count;
                    break;
                case ConsoleKey.Enter:
                    return selectedIndex;
                case ConsoleKey.Escape:
                    return -1;
            }

        } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);
        
        return selectedIndex;
    }
    
    private ConnectionInfo CreateNewConnection()
    {
        Console.Clear();
        _themeManager.WriteLine("╔══════════════════════════════════════════════════════════════╗", ThemeStyle.Border);
        _themeManager.WriteLine("║                        新增資料庫連線                        ║", ThemeStyle.Heading);
        _themeManager.WriteLine("╚══════════════════════════════════════════════════════════════╝", ThemeStyle.Border);
        Console.WriteLine();
        
        var connection = new ConnectionInfo();
        
        _themeManager.Write("連線名稱 (可選): ", ThemeStyle.Label);
        connection.DisplayName = Console.ReadLine() ?? string.Empty;
        
        _themeManager.Write("主機位址 [localhost]: ", ThemeStyle.Label);
        var host = Console.ReadLine();
        connection.Host = string.IsNullOrEmpty(host) ? "localhost" : host;
        
        _themeManager.Write("連接埠 [5432]: ", ThemeStyle.Label);
        var portStr = Console.ReadLine();
        if (int.TryParse(portStr, out int port))
        {
            connection.Port = port;
        }
        
        _themeManager.Write("使用者名稱: ", ThemeStyle.Label);
        connection.Username = Console.ReadLine() ?? string.Empty;
        
        _themeManager.Write("密碼: ", ThemeStyle.Label);
        connection.Password = ReadPassword();
        
        _themeManager.Write("預設資料庫 [postgres]: ", ThemeStyle.Label);
        var database = Console.ReadLine();
        connection.DefaultDatabase = string.IsNullOrEmpty(database) ? "postgres" : database;
        
        _themeManager.Write("儲存密碼? (y/N): ", ThemeStyle.Label);
        var savePassword = Console.ReadLine();
        connection.SavePassword = savePassword?.ToLower() == "y" || savePassword?.ToLower() == "yes";
        
        return connection;
    }
    
    private string ReadPassword()
    {
        var password = string.Empty;
        ConsoleKeyInfo keyInfo;
        
        do
        {
            keyInfo = Console.ReadKey(true);
            
            if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
        } while (keyInfo.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password;
    }

    public bool ShowConfirmDialog(string message, string title = "確認")
    {
        Console.WriteLine();
        _themeManager.WriteLine($"╔══ {title} ══╗", ThemeStyle.Border);
        _themeManager.WriteLine($"║ {message} ║", ThemeStyle.Info);
        _themeManager.WriteLine("╚═══════════════════════════════════════╝", ThemeStyle.Border);
        _themeManager.Write("確定嗎? (y/N): ", ThemeStyle.Label);
        
        var response = Console.ReadLine();
        return response?.ToLower() == "y" || response?.ToLower() == "yes";
    }
}

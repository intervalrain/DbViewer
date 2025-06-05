using Domain.Shared.Themes;
using Domain.Themes.Entities;

namespace Domain.Themes;

public class ThemeManager
{
    private int _currentTheme = 0;
    private readonly List<ThemeDefinition> _themes = [];
    
    public string CurrentThemeName => _themes[_currentTheme].Name!;
    
    public ThemeManager()
    {
        InitializeThemes();
    }
    
    private void InitializeThemes()
    {
        _themes.Add(new ThemeDefinition
        {
            Name = "深夜藍",
            StyleDefinitions = new Dictionary<ThemeStyle, ConsoleColor[]>
            {
                { ThemeStyle.Normal, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.Heading, new[] { ConsoleColor.Cyan, ConsoleColor.Black } },
                { ThemeStyle.SubHeading, new[] { ConsoleColor.DarkCyan, ConsoleColor.Black } },
                { ThemeStyle.Success, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.Error, new[] { ConsoleColor.Red, ConsoleColor.Black } },
                { ThemeStyle.ErrorDetail, new[] { ConsoleColor.DarkRed, ConsoleColor.Black } },
                { ThemeStyle.Info, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.Command, new[] { ConsoleColor.Yellow, ConsoleColor.Black } },
                { ThemeStyle.Prompt, new[] { ConsoleColor.Magenta, ConsoleColor.Black } },
                { ThemeStyle.Continuation, new[] { ConsoleColor.DarkMagenta, ConsoleColor.Black } },
                { ThemeStyle.Label, new[] { ConsoleColor.Cyan, ConsoleColor.Black } },
                { ThemeStyle.Value, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.DatabaseName, new[] { ConsoleColor.Blue, ConsoleColor.Black } },
                { ThemeStyle.Time, new[] { ConsoleColor.DarkYellow, ConsoleColor.Black } },
                { ThemeStyle.LineNumber, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.MenuOption, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.MenuSelected, new[] { ConsoleColor.Black, ConsoleColor.Cyan } },
                { ThemeStyle.Border, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.Highlight, new[] { ConsoleColor.Yellow, ConsoleColor.Black } }
            },
            TableColors = new Dictionary<string, ConsoleColor[]>
            {
                { "HeaderFg", new[] { ConsoleColor.White, ConsoleColor.Black } },
                { "HeaderBg", new[] { ConsoleColor.DarkBlue, ConsoleColor.Black } },
                { "BorderColor", new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { "RowColorEven", new[] { ConsoleColor.White, ConsoleColor.Black } },
                { "RowColorOdd", new[] { ConsoleColor.Gray, ConsoleColor.Black } }
            }
        });
        
        _themes.Add(new ThemeDefinition
        {
            Name = "翠綠",
            StyleDefinitions = new Dictionary<ThemeStyle, ConsoleColor[]>
            {
                { ThemeStyle.Normal, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.Heading, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.SubHeading, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Success, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Error, new[] { ConsoleColor.Red, ConsoleColor.Black } },
                { ThemeStyle.ErrorDetail, new[] { ConsoleColor.DarkRed, ConsoleColor.Black } },
                { ThemeStyle.Info, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.Command, new[] { ConsoleColor.DarkYellow, ConsoleColor.Black } },
                { ThemeStyle.Prompt, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.Continuation, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Label, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.Value, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.DatabaseName, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Time, new[] { ConsoleColor.Yellow, ConsoleColor.Black } },
                { ThemeStyle.LineNumber, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.MenuOption, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.MenuSelected, new[] { ConsoleColor.Black, ConsoleColor.Green } },
                { ThemeStyle.Border, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Highlight, new[] { ConsoleColor.Yellow, ConsoleColor.Black } }
            },
            TableColors = new Dictionary<string, ConsoleColor[]>
            {
                { "HeaderFg", new[] { ConsoleColor.White, ConsoleColor.Black } },
                { "HeaderBg", new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { "BorderColor", new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { "RowColorEven", new[] { ConsoleColor.White, ConsoleColor.Black } },
                { "RowColorOdd", new[] { ConsoleColor.Gray, ConsoleColor.Black } }
            }
        });
        
        _themes.Add(new ThemeDefinition
        {
            Name = "黑客風格",
            StyleDefinitions = new Dictionary<ThemeStyle, ConsoleColor[]>
            {
                { ThemeStyle.Normal, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.Heading, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.SubHeading, new[] { ConsoleColor.Gray, ConsoleColor.Black } },
                { ThemeStyle.Success, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Error, new[] { ConsoleColor.Red, ConsoleColor.Black } },
                { ThemeStyle.ErrorDetail, new[] { ConsoleColor.DarkRed, ConsoleColor.Black } },
                { ThemeStyle.Info, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.Command, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.Prompt, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.Continuation, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Label, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.Value, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.DatabaseName, new[] { ConsoleColor.Gray, ConsoleColor.Black } },
                { ThemeStyle.Time, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.LineNumber, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.MenuOption, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.MenuSelected, new[] { ConsoleColor.Black, ConsoleColor.Green } },
                { ThemeStyle.Border, new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { ThemeStyle.Highlight, new[] { ConsoleColor.White, ConsoleColor.Black } }
            },
            TableColors = new Dictionary<string, ConsoleColor[]>
            {
                { "HeaderFg", new[] { ConsoleColor.Black, ConsoleColor.Black } },
                { "HeaderBg", new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { "BorderColor", new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } },
                { "RowColorEven", new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { "RowColorOdd", new[] { ConsoleColor.DarkGreen, ConsoleColor.Black } }
            }
        });
        
        _themes.Add(new ThemeDefinition
        {
            Name = "優雅紫色",
            StyleDefinitions = new Dictionary<ThemeStyle, ConsoleColor[]>
            {
                { ThemeStyle.Normal, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.Heading, new[] { ConsoleColor.Magenta, ConsoleColor.Black } },
                { ThemeStyle.SubHeading, new[] { ConsoleColor.DarkMagenta, ConsoleColor.Black } },
                { ThemeStyle.Success, new[] { ConsoleColor.Green, ConsoleColor.Black } },
                { ThemeStyle.Error, new[] { ConsoleColor.Red, ConsoleColor.Black } },
                { ThemeStyle.ErrorDetail, new[] { ConsoleColor.DarkRed, ConsoleColor.Black } },
                { ThemeStyle.Info, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.Command, new[] { ConsoleColor.Yellow, ConsoleColor.Black } },
                { ThemeStyle.Prompt, new[] { ConsoleColor.Magenta, ConsoleColor.Black } },
                { ThemeStyle.Continuation, new[] { ConsoleColor.DarkMagenta, ConsoleColor.Black } },
                { ThemeStyle.Label, new[] { ConsoleColor.Magenta, ConsoleColor.Black } },
                { ThemeStyle.Value, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.DatabaseName, new[] { ConsoleColor.DarkMagenta, ConsoleColor.Black } },
                { ThemeStyle.Time, new[] { ConsoleColor.DarkYellow, ConsoleColor.Black } },
                { ThemeStyle.LineNumber, new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { ThemeStyle.MenuOption, new[] { ConsoleColor.White, ConsoleColor.Black } },
                { ThemeStyle.MenuSelected, new[] { ConsoleColor.Black, ConsoleColor.Magenta } },
                { ThemeStyle.Border, new[] { ConsoleColor.DarkMagenta, ConsoleColor.Black } },
                { ThemeStyle.Highlight, new[] { ConsoleColor.Yellow, ConsoleColor.Black } }
            },
            TableColors = new Dictionary<string, ConsoleColor[]>
            {
                { "HeaderFg", new[] { ConsoleColor.White, ConsoleColor.Black } },
                { "HeaderBg", new[] { ConsoleColor.DarkMagenta, ConsoleColor.Black } },
                { "BorderColor", new[] { ConsoleColor.DarkGray, ConsoleColor.Black } },
                { "RowColorEven", new[] { ConsoleColor.White, ConsoleColor.Black } },
                { "RowColorOdd", new[] { ConsoleColor.Gray, ConsoleColor.Black } }
            }
        });
    }
    
    public void SetTheme(int themeIndex)
    {
        if (themeIndex >= 0 && themeIndex < _themes.Count)
        {
            _currentTheme = themeIndex;
        }
    }
    
    public void SetTheme(string themeName)
    {
        var themeIndex = _themes.FindIndex(t => t.Name == themeName);
        if (themeIndex >= 0)
        {
            _currentTheme = themeIndex;
        }
    }
    
    public void CycleTheme()
    {
        _currentTheme = (_currentTheme + 1) % _themes.Count;
    }
    
    public void Write(string text, ThemeStyle style)
    {
        var colors = _themes[_currentTheme].StyleDefinitions[style];
        Console.ForegroundColor = colors[0];
        if (colors.Length > 1 && colors[1] != ConsoleColor.Black)
        {
            Console.BackgroundColor = colors[1];
        }
        Console.Write(text);
        Console.ResetColor();
    }
    
    public void WriteLine(string text, ThemeStyle style)
    {
        Write(text, style);
        Console.WriteLine();
    }
    
    public void WriteTableHeader(string[] columnNames, int[] columnWidths)
    {
        var headerFg = _themes[_currentTheme].TableColors["HeaderFg"][0];
        var headerBg = _themes[_currentTheme].TableColors["HeaderBg"][0];
        var borderColor = _themes[_currentTheme].TableColors["BorderColor"][0];

        Console.ForegroundColor = borderColor;
        Console.Write("┌");
        for (int i = 0; i < columnWidths.Length; i++)
        {
            Console.Write(new string('─', columnWidths[i]));
            if (i < columnWidths.Length - 1)
                Console.Write("┬");
        }
        Console.WriteLine("┐");

        Console.ForegroundColor = borderColor;
        Console.Write("│");
        Console.ForegroundColor = headerFg;
        Console.BackgroundColor = headerBg;
        
        for (int i = 0; i < columnNames.Length; i++)
        {
            string paddedName = columnNames[i].PadRight(columnWidths[i] - 2);
            Console.Write(" " + paddedName + " ");
            Console.ResetColor();
            Console.ForegroundColor = borderColor;
            Console.Write("│");
            if (i < columnNames.Length - 1)
            {
                Console.ForegroundColor = headerFg;
                Console.BackgroundColor = headerBg;
            }
        }

        Console.WriteLine();
        Console.ResetColor();

        Console.ForegroundColor = borderColor;
        Console.Write("├");
        for (int i = 0; i < columnWidths.Length; i++)
        {
            Console.Write(new string('─', columnWidths[i]));
            if (i < columnWidths.Length - 1)
                Console.Write("┼");
        }
        Console.WriteLine("┤");

        Console.ResetColor();
    }
    
    public void WriteTableRow(string[] values, int[] columnWidths, bool isEven)
    {
        var rowColor = isEven ? 
            _themes[_currentTheme].TableColors["RowColorEven"][0] : 
            _themes[_currentTheme].TableColors["RowColorOdd"][0];
        var borderColor = _themes[_currentTheme].TableColors["BorderColor"][0];

        Console.ForegroundColor = borderColor;
        Console.Write("│");
        Console.ForegroundColor = rowColor;
        
        for (int i = 0; i < values.Length; i++)
        {
            string paddedValue = values[i].PadRight(columnWidths[i] - 2);
            Console.Write(" " + paddedValue + " ");
            Console.ForegroundColor = borderColor;
            Console.Write("│");
            Console.ForegroundColor = rowColor;
        }

        Console.WriteLine();
        Console.ResetColor();
    }
    
    public void WriteTableFooter(int[] columnWidths)
    {
        var borderColor = _themes[_currentTheme].TableColors["BorderColor"][0];

        Console.ForegroundColor = borderColor;
        Console.Write("└");
        for (int i = 0; i < columnWidths.Length; i++)
        {
            Console.Write(new string('─', columnWidths[i]));
            if (i < columnWidths.Length - 1)
                Console.Write("┴");
        }
        Console.WriteLine("┘");

        Console.ResetColor();
    }
    
    public List<string> GetAvailableThemes()
    {
        return _themes.Select(t => t.Name!).ToList();
    }
}


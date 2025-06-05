using Domain.Shared.Themes;

namespace Domain.Themes.Entities;

public class ThemeDefinition
{
    public string? Name { get; set; }
    public Dictionary<ThemeStyle, ConsoleColor[]> StyleDefinitions { get; set; } = [];
    public Dictionary<string, ConsoleColor[]> TableColors { get; set; } = [];
}
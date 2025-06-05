namespace Common.Modularity;

public class ConfigureAction
{
    public int Order { get; init; }
    public string Name { get; init; }
    public Type ModuleType { get; init; }
    public DateTime Timestamp { get; init; }
    public string? PrevModuleName { get; init; }
    public Type? PrevModuleType { get; init; }
}
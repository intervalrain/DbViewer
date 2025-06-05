using Common.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Modularity;

public class ServiceConfigurationContext
{
    private IConfiguration? _configuration;
    private readonly List<ConfigureAction> _configures = [];

    public IServiceCollection Services { get; }
    public IConfiguration Configuration => _configuration ??= Services.GetConfiguration();
    public IDictionary<string, object?> Items { get; }
    public int Count => Items.Count;
    public IReadOnlyList<ConfigureAction> Configures => _configures.AsReadOnly();

    public object? this[string key]
    {
        get => Items.GetOrDefault(key);
        set => Items[key] = value;
    }

    public ServiceConfigurationContext([NotNull] IServiceCollection services)
    {
        Services = Check.NotNull(services, nameof(services));
        Items = new Dictionary<string, object?>();
    }

    public void AddConfigureAction(IModule module)
    {
        _configures.Add(new ConfigureAction
        {
            Order = _configures.Count + 1,
            Name = module.Name,
            ModuleType = module.GetType(),
            Timestamp = DateTime.UtcNow,
            PrevModuleName = _configures.LastOrDefault()?.Name ?? string.Empty,
            PrevModuleType = _configures.LastOrDefault()?.ModuleType ?? null
        });
    }
}
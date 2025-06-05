namespace Common.Modularity;

public abstract class Module : IModule
{
    protected internal ServiceConfigurationContext ServiceConfigurationContext
    {
        get => _serviceConfigurationContext ?? throw new Exception($"{nameof(ServiceConfigurationContext)} is only available in the {nameof(ConfigureServices)} method.");
        internal set => _serviceConfigurationContext = value;
    }

    private ServiceConfigurationContext _serviceConfigurationContext;

    public string Name
    {
        get
        {
            var name = GetType().Name;
            return name.EndsWith(nameof(Module)) ? name[..^nameof(Module).Length] ?? Guid.NewGuid().ToString() : name;
        }
    }

    public static bool IsModule(Type type)
    {
        return type.IsClass &&
               !type.IsAbstract &&
               !type.IsGenericType &&
               type.IsAssignableTo<IModule>();
    }

    public virtual void ConfigureServices(ServiceConfigurationContext services)
    {
    }
}
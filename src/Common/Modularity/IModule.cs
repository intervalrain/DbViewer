namespace Common.Modularity;

public interface IModule
{
    public string Name { get; }
    void ConfigureServices(ServiceConfigurationContext services);
}
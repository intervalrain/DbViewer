using System.Text.Json;

using Common.Modularity;

using Microsoft.Extensions.DependencyInjection;

namespace Common;

public class CommonModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = true;
        });
    }
}
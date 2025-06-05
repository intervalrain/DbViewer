using Application;
using Application.Contracts;

using Common;
using Common.Modularity;
using Common.Modularity.Attributes;
using Infrastructure;

namespace Presentation.Console;

[DependsOn(
    typeof(ApplicationModule),
    typeof(ApplicationContractsModule),
    typeof(InfrastructureModule),
    typeof(CommonModule)
    )]
public class PresentationConsoleModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}

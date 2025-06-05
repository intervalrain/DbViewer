using Application;
using Application.Contracts;
using Common;
using Common.Modularity;
using Common.Modularity.Attributes;
using Infrastructure;

namespace Presentation.WebApi;

[DependsOn(
    typeof(ApplicationModule),
    typeof(ApplicationContractsModule),
    typeof(InfrastructureModule),
    typeof(CommonModule)
    )]
public class PresentationWebApiModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}

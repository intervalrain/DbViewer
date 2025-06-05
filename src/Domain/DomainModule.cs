using Common;
using Common.Modularity;
using Common.Modularity.Attributes;
using Domain.Shared;

namespace Domain;

[DependsOn(
    typeof(DomainSharedModule),
    typeof(CommonModule)
    )]
public class DomainModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}

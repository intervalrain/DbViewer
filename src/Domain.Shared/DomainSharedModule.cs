using Common;
using Common.Modularity;
using Common.Modularity.Attributes;

namespace Domain.Shared;

[DependsOn(
    typeof(CommonModule)
)]
public class DomainSharedModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}

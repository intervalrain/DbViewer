using Common;
using Common.Modularity;
using Common.Modularity.Attributes;
using Domain;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Contracts;

[DependsOn(
    typeof(DomainModule),
    typeof(DomainSharedModule),
    typeof(CommonModule)
    )]
public class ApplicationContractsModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapper(typeof(ApplicationAutoMappingProfile));
    }
}

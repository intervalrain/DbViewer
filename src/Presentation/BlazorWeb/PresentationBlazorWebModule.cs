using Microsoft.Extensions.DependencyInjection;
using Common.Modularity;
using Common.Modularity.Attributes;
using Application;
using Infrastructure;
using Presentation.BlazorWeb.ViewModels;
using Application.Contracts;
using Common;

namespace Presentation.BlazorWeb;

[DependsOn(
    typeof(ApplicationModule),
    typeof(ApplicationContractsModule),
    typeof(InfrastructureModule),
    typeof(CommonModule)
    )]
public class PresentationBlazorWebModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<LoginViewModel>();
        context.Services.AddScoped<DatabaseViewModel>();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Common.Modularity;
using Common.Modularity.Attributes;
using Domain;
using Domain.Themes;
using Domain.Menu;
using Domain.History;
using Application.Contracts;
using Application.Contracts.Services;
using Application.Services;
using Domain.Configuration.Entities;
using Common;

namespace Application;

[DependsOn(
    typeof(ApplicationContractsModule),
    typeof(DomainModule),
    typeof(CommonModule)
    )]
public class ApplicationModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IApplicationService, ApplicationService>();
        context.Services.AddSingleton<IAuthService, AuthService>();
        context.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        context.Services.AddScoped<ICookieService, CookieService>();
        
        context.Services.AddSingleton<ThemeManager>();
        context.Services.AddSingleton<MenuManager>();
        context.Services.AddSingleton(provider => 
        {
            var configService = provider.GetRequiredService<IConfigurationService>();
            return new HistoryManager(configService.Application.HistorySize);
        });
        context.Services.AddSingleton(provider => AppSettings.Load());
    }
} 

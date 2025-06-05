using Application.Contracts.Services;


using AutoMapper;

using Domain.Configuration.Entities;
using Domain.History;
using Domain.Menu;
using Domain.Themes;


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Base;

public abstract class ApplicationServiceBase : IApplicationService
{
    protected readonly ILogger<ApplicationServiceBase> _logger;
    protected readonly IMapper _mapper;
    protected readonly IDatabaseService _databaseService;
    protected readonly IConfigurationService _configurationService;
    protected readonly ThemeManager _themeManager;
    protected readonly MenuManager _menuManager;
    protected readonly HistoryManager _historyManager;
    protected readonly AppSettings _appSettings;

    // TODO: lazy loading
    public ApplicationServiceBase(IServiceProvider sp)
    {
        _logger = sp.GetRequiredService<ILogger<ApplicationServiceBase>>();
        _mapper = sp.GetRequiredService<IMapper>();
        _databaseService = sp.GetRequiredService<IDatabaseService>();
        _configurationService = sp.GetRequiredService<IConfigurationService>();
        _themeManager = sp.GetRequiredService<ThemeManager>();
        _menuManager = sp.GetRequiredService<MenuManager>();
        _historyManager = sp.GetRequiredService<HistoryManager>();
        _appSettings = sp.GetRequiredService<AppSettings>();

        InitializeApplication();
    }

    public virtual void InitializeApplication()
    {
        _themeManager.SetTheme(_configurationService.Application.DefaultTheme);
        _historyManager.LoadFromFile(_configurationService.Application.HistoryFilePath);
    }

    public abstract Task RunAsync();
}
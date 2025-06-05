using Application.Contracts.Dtos;
using Application.Contracts.Services;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    
    public AppConfiguration Application { get; }
    public DatabaseConfiguration Database { get; }
    public LoggingConfiguration Logging { get; }

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        Application = new AppConfiguration();
        _configuration.GetSection("Application").Bind(Application);
        
        Database = new DatabaseConfiguration();
        _configuration.GetSection("Database").Bind(Database);
        
        Logging = new LoggingConfiguration();
        _configuration.GetSection("Logging").Bind(Logging);
    }
} 
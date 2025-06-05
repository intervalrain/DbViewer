using Application.Contracts.Dtos;

namespace Application.Contracts.Services;

public interface IConfigurationService
{
    AppConfiguration Application { get; }
    DatabaseConfiguration Database { get; }
    LoggingConfiguration Logging { get; }
}
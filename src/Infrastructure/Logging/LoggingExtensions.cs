using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Application.Contracts.Services;

namespace Infrastructure.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder, IConfigurationService configurationService)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            var loggingConfig = configurationService.Logging;
            
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: loggingConfig.Console.OutputTemplate)
                .WriteTo.File(
                    path: loggingConfig.File.Path,
                    outputTemplate: loggingConfig.File.OutputTemplate,
                    rollingInterval: ParseRollingInterval(loggingConfig.File.RollingInterval),
                    retainedFileCountLimit: loggingConfig.File.RetainedFileCountLimit);
        });
    }

    private static RollingInterval ParseRollingInterval(string interval)
    {
        return interval.ToLowerInvariant() switch
        {
            "infinite" => RollingInterval.Infinite,
            "year" => RollingInterval.Year,
            "month" => RollingInterval.Month,
            "day" => RollingInterval.Day,
            "hour" => RollingInterval.Hour,
            "minute" => RollingInterval.Minute,
            _ => RollingInterval.Day
        };
    }
}

using Common.Modularity;
using Serilog;
using Application.Contracts.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            var applicationService = scope.ServiceProvider.GetRequiredService<IApplicationService>();
            await applicationService.RunAsync();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"應用程式啟動失敗: {ex.Message}");
            Environment.Exit(1);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddEnvironmentVariables()
                      .AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                services.ConfigureModules(typeof(PresentationConsoleModule));
            })
            // .ConfigureLogging((context, logging) =>
            // {
            //     logging.ClearProviders();
            // })
            .UseSerilog((context, services, configuration) =>
            {
                var configurationService = services.GetRequiredService<IConfigurationService>();
                var loggingConfig = configurationService.Logging;

                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: loggingConfig.Console.OutputTemplate)
                    .WriteTo.File(
                        path: loggingConfig.File.Path,
                        outputTemplate: loggingConfig.File.OutputTemplate,
                        rollingInterval: ParseRollingInterval(loggingConfig.File.RollingInterval),
                        retainedFileCountLimit: loggingConfig.File.RetainedFileCountLimit);
            });

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

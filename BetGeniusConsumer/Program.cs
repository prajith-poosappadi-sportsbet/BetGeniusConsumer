using BetGeniusConsumer.Interfaces;
using BetGeniusConsumer.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace BetGeniusConsumer;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = new ConfigurationBuilder();
        builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        //only add secrets in development
        var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
        var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                            devEnvironmentVariable.ToLower() == "development";
        if (isDevelopment) 
        {
            builder
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();
        }
        
        var config = builder.Build();
        
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
                services.AddScoped<ILogger, Logger>();
                services.AddScoped<IConsumer, AblyFeedConsumer>();
                services.AddScoped<IAnalysis, Analysis>();
                services.AddScoped<IAnalyticsStore, AnalyticsStoreCsv>();
                services.Configure<BetGeniusClient>(config.GetSection(nameof(BetGeniusClient)))
                    .AddOptions()
                    .AddLogging()
                    .AddSingleton<ISecretRevealer, SecretRevealer>();
            });
    }
}
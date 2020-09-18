using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using OpenFTTH.DesktopBridge.Bridge;
using System.Net;

namespace OpenFTTH.DesktopBridge.Internal
{
    public static class HostConfig
    {
        public static IHost Configure()
        {
            var hostBuilder = new HostBuilder();
            ConfigureApp(hostBuilder);
            ConfigureServices(hostBuilder);
            ConfigureLogging(hostBuilder);

            return hostBuilder.Build();
        }

        private static void ConfigureApp(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
            });
        }

      private static void ConfigureServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.AddHostedService<DesktopBridgeHost>();

                services.AddTransient<IBridgeSessionFactory, BridgeSessionFactory>();

                services.AddSingleton<BridgeServer>(
                    x => new BridgeServer(
                        IPAddress.Any,
                        5000,
                        x.GetRequiredService<IBridgeSessionFactory>(),
                        x.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BridgeServer>>()));
            });
        }

        private static void ConfigureLogging(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var loggingConfiguration = new ConfigurationBuilder()
                   .AddEnvironmentVariables().Build();

                services.AddLogging(loggingBuilder =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(loggingConfiguration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .CreateLogger();

                    loggingBuilder.AddSerilog(logger, true);
                });
            });
        }
    }
}

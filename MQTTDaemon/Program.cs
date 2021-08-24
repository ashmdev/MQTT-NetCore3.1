using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Server;
namespace MQTTDaemon
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<MQTTConfig>(hostContext.Configuration.GetSection("MQTT"));
                    services.AddSingleton<IHostedService, MqttService>();
                })
                .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConsole();
                });
            await builder.RunConsoleAsync();
        }
    }
}
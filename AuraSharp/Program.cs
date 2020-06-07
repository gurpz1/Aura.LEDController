using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AuraSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, args);
            using (var services = serviceCollection.BuildServiceProvider())
            {
                var controller = services.GetService<AuraUsbController>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                IList<LED> ledColours = new List<LED>(120);
                for (var j =0; j < 120; j++)
                {
                    ledColours.Insert(j,new LED(255,97,41));
                }
                controller.DirectControl(ledColours);


                logger.LogInformation(("Done"));
            }
        }
        private static void ConfigureServices(IServiceCollection serviceCollection, string[] args)
        {
            // Read configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddCommandLine(args)
                .Build();

            // Intialise Logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            
            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            });

            serviceCollection.AddSingleton<AuraUsbController>(services => new AuraUsbController(services.GetRequiredService<ILogger<AuraUsbController>>(), 0));
        }
    }
}
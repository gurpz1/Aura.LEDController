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
                DoWithController(services);
                Console.ReadKey();
            }
        }

        private static void DoWithController(ServiceProvider services)
        {
            var a = services.GetService<AddressableLedController>();

            var leds = new List<LED>(AddressableLedController.MaxLeds);
            for (int i = 0; i < AddressableLedController.MaxLeds; i++)
            {
                leds.Add(new LED(0,0,0));
            }
            a.SetLeds(leds, 0);

            for (byte i = 0; i < 255; i++)
            {
                leds = leds.Select(x =>
                {
                    x.R = i;
                    x.G = i;
                    x.B = i;
                    return x;
                }).ToList();
                
                a.SetLeds(leds, 0);
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

            serviceCollection.AddSingleton<AddressableLedController>();
        }
    }
}
using System;
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
                var logger = services.GetRequiredService<ILogger<Program>>();
                
                logger.LogInformation("Hello World");

                var localDevices = DeviceList.Local;
                foreach (var device in localDevices.GetHidDevices(0x0B05))
                {
                    if (device.GetProductName() == "AURA LED Controller")
                    {
                        logger.LogDebug("Success");
                        DoSomethingFancy(logger, device);
                    }
                }
            }
        }

        private static void DoSomethingFancy(ILogger logger, HidDevice device)
        {
            var messageLength = 65;
            byte messageStart = 0xEC;
            var message = new byte[messageLength];
            Array.Fill(message, Byte.MinValue);
            message[0] = messageStart;
            message[1] = (byte) AuraMode.EFFECT;
            message[4] = 0xff;

            WriteDevice(logger, device, message);

            byte start_led = 15;
            byte end_led = 30;
            
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x00;
            message[3] =start_led;
            message[4] = (byte) (end_led - start_led);
            for (int i = 5; i < messageLength; i += 3)
            {
                message[i] = 255;
            }
            
            WriteDevice(logger, device, message);

            message[2] = 0x80;
            WriteDevice(logger, device, message);
        }

        private static void WriteDevice(ILogger logger, HidDevice device, byte[] message)
        {
            using (var deviceStream = device.Open())
            {
                logger.LogInformation($"Writing: {message}");
                logger.LogInformation($"Device is writeable? {deviceStream.CanWrite}");
                deviceStream.Write(message,0,65);
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
        }
    }
}
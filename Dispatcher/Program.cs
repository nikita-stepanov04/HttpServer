using Configuration;
using DispatcherToolKit;
using DispatcherToolKit.Handlers;
using HttpServerCore;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System.Text.Json;
using WebToolkit.Models;

namespace Dispatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = Convert.ToInt32(args[0]);
            ILoggerFactory loggerFactory = new SerilogLoggerFactory(CommonSettingsConfiguration.Logger);

            DispatcherConnection.DispatcherPort = port;
            var dispatcher = new HttpServerBuilder(DispatcherConnection.DispatcherPort,
                loggerFactory, ProcessingMode.MultiThread);

            dispatcher.UseEndpoints();

            dispatcher.MapGet("/register", DispatcherEndpoints.RegisterServer);
            dispatcher.MapGet("/unregister", DispatcherEndpoints.UnregisterServer);
            dispatcher.MapGet("/get", DispatcherEndpoints.GetAddress);

            using HttpServer dispatcherServer = dispatcher.Build();

            dispatcherServer.StartAsync().Wait();
        }
    }
}

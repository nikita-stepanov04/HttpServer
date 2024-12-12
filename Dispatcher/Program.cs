using Configuration;
using DispatcherToolKit;
using DispatcherToolKit.Handlers;
using HttpServerCore;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using WebToolkit.Models;

namespace Dispatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = 8080;
            if (args.Length > 0)
            {
                port = Convert.ToInt32(args[0]);
            }
            ILoggerFactory loggerFactory = new SerilogLoggerFactory(CommonSettingsConfiguration.Logger);

            DispatcherConnection.DispatcherPort = port;
            var app = new HttpServerBuilder(DispatcherConnection.DispatcherPort,
                loggerFactory, ProcessingMode.MultiThread);

            app.UseEndpoints();

            app.MapGet("/register", DispatcherEndpoints.RegisterServer);
            app.MapGet("/unregister", DispatcherEndpoints.UnregisterServer);
            app.MapGet("/get", DispatcherEndpoints.GetAddress);

            using HttpServer dispatcherServer = app.Build();

            dispatcherServer.StartAsync().Wait();
        }
    }
}

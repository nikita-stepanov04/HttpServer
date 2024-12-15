using Configuration;
using DispatcherToolKit.Handlers;
using DispatcherToolKit.Middleware;
using HttpServerCore;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using WebToolkit.Models;
using static WebApp.Endpoints.Endpoints;
using static WebApp.WebAppHelper;

namespace WebApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int port = 0;
            if (args.Length > 0)
            {
                port = Convert.ToInt32(args[0]);
                DispatcherConnection.DispatcherPort = Convert.ToInt32(args[1]);
            }
            else
            {
                port = 8080;
            }

            ILoggerFactory loggerFactory = new SerilogLoggerFactory(CommonSettingsConfiguration.Logger);
            HttpServerBuilder app = new(port, loggerFactory, ProcessingMode.MultiThread);

            app.UseErrorMiddleware();
            app.UseRequestForwarding();
            app.UseEndpoints();

            app.MapViewsAssemblyType(typeof(Program));
            app.PrecompileViews();

            app.MapStaticPath(ContentPath);
            app.MapErrorPath(ContentPath + "/errors");

            app.MapGet("/test-html", TestHtml);
            app.MapGet("/test-json", TestJson);
            app.MapGet("/test-razor", TestRazor);
            app.MapGet("/test-status", TestStatus);

            app.MapGet("/error", _ => throw new Exception());
            app.MapPost("/fileLength", FileLength);

            app.AddOnServerStartedEventHandler<ServerStartedEventHandler>();
            app.AddOnServerStoppedEventHandler<ServerStoppedEventHandler>();

            using HttpServer server = app.Build();

            await server.StartAsync();
        }
    }
}

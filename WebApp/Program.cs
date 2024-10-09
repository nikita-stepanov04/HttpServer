using HttpServerCore;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using HttpServerCore.Handlers;

namespace WebApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()                
                .WriteTo.Console()                    
                .CreateLogger();

            using ILoggerFactory loggerFactory = new SerilogLoggerFactory();

            IEndpointProvider endpointProvider = new EndpointProvider();

            endpointProvider.MapStaticPath(AppContext.BaseDirectory + "wwwroot");
            endpointProvider.MapErrorPath(AppContext.BaseDirectory + "wwwroot/errors");

            endpointProvider.MapGet("/test", async (request, response) =>
            {
                using var sw = new StreamWriter(response.Content, leaveOpen: true);
                string result = "Hello, World!";

                response.Headers.Set("Content-Type", "text/plain");
                response.Headers.Set("Content-Length", result.Length.ToString());
                await sw.WriteAsync(result);
                await sw.FlushAsync();
                response.Content.Position = 0;
            });

            using HttpServer server = new(8080, loggerFactory, endpointProvider);
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    server.Stop();
                };
                await server.StartAsync();
            }            
        }
    }
}

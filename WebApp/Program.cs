using HttpServerCore;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using HttpServerCore.Handlers;
using System.Text;
using Microsoft.Extensions.Primitives;

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

                StringBuilder builder = new("Query params: ");
                foreach(var param in request.QueryParams)
                {
                    builder.Append($"{param.Key}: {param.Value}, ");
                }
                string result = builder.ToString();

                response.Headers.Set("Content-Type", "text/plain");
                await sw.WriteAsync(result);
                await sw.FlushAsync();
            });

            endpointProvider.MapPost("/fileLength", async (request, response) =>
            {
                if (request.Content != null)
                {
                    long count = request.Content.Length;
                    using var sw = new StreamWriter(response.Content, leaveOpen: true);

                    string result = $"File length is: {count} bytes";

                    response.Headers.Set("Content-Type", "text/plain");

                    await sw.WriteLineAsync(result);
                    await sw.FlushAsync();
                }
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

using HttpServerCore;
using Serilog;

namespace WebApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();            

            using HttpServer server = new(8080, Log.Logger);
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

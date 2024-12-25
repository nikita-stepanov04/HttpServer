using DispatcherToolKit;
using HttpServerCore.Server;
using WebToolkit.Server;
using static Configuration.CommonSettingsConfiguration;

namespace Dispatcher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int port = 8080;
            if (args.Length > 0)
            {
                port = Convert.ToInt32(args[0]);
            }

            IHttpServerBuilder app = new HttpServerBuilder(port, SerilogLoggerFactory);

            app.UseEndpoints();
            app.MapDispatcherEndpoints();

            using HttpServer dispatcherServer = app.Build();
            await dispatcherServer.StartAsync();
        }
    }
}

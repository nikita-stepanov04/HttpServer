using HttpServerCore;
using HttpServerCore.Mediators;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text.Json;

namespace DispatcherToolKit.Handlers
{
    public class ServerStartedEventHandler : IEventHandler<ServerStartedEvent>
    {
        public async Task HandleAsync(ServerStartedEvent e, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ServerStartedEvent>();
            logger.LogInformation("Notify dispatcher that the server has started on the port: {p}", e.Port);

            using var client = new HttpServerCore.HttpClient(loggerFactory);
            var request = new HttpRequest()
            {
                Method = "GET",
                Uri = "/register"
            };
            await request.WriteJsonAsync(e.Port);

            try
            {
                await client.SendAsync(request, new HttpResponse(), DispatcherConnection.DispatcherPort);
            }
            catch (SocketException)
            {
                logger.LogError("Failed to notify dispatcher");
            }
        }
    }
}

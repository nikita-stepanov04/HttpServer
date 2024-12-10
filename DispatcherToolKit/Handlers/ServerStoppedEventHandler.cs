using HttpServerCore;
using HttpServerCore.Mediators;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DispatcherToolKit.Handlers
{
    public class ServerStoppedEventHandler : IEventHandler<ServerStoppedEvent>
    {
        public async Task HandleAsync(ServerStoppedEvent e, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ServerStartedEvent>();
            logger.LogInformation("Notify dispatcher that the server has stopped");

            using var client = new HttpServerCore.HttpClient(loggerFactory);

            var request = new HttpRequest()
            {
                Method = "GET",
                Uri = "/unregister"
            };

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

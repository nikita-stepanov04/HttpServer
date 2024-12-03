using HttpServerCore.Mediators;
using Microsoft.Extensions.Logging;

namespace WebToolkit.Handlers
{
    public class ServerStartedEventHandler : IEventHandler<ServerStartedEvent>
    {
        public Task HandleAsync(ServerStartedEvent e, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ServerStartedEvent>();
            logger.LogInformation("Notify dispatcher that the server has started on the port: {p}", e.Port);
            return Task.CompletedTask;
        }
    }
}

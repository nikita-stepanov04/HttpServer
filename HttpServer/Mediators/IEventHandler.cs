using Microsoft.Extensions.Logging;

namespace HttpServerCore.Mediators
{
    public interface IEventHandler<IEvent>
    {
        Task HandleAsync(IEvent e, ILoggerFactory loggerFactory);
    }
}

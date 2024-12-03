using Microsoft.Extensions.Logging;

namespace HttpServerCore.Mediators
{
    public class Mediator
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly Dictionary<Type, List<object>> _eventHandlers = new();

        public Mediator(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Register<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new();
            }
            _eventHandlers[eventType].Add(handler);
        }

        public async Task RaiseAsync<TEvent>(TEvent e) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType])
                {
                    await ((IEventHandler<TEvent>)handler).HandleAsync(e, _loggerFactory);
                }
            }
        }
    }
}

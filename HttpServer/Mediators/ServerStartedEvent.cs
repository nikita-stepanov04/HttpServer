namespace HttpServerCore.Mediators
{
    public class ServerStartedEvent : IEvent
    {
        public int Port { get; set; }

        public ServerStartedEvent(int port) => Port = port;
    }
}

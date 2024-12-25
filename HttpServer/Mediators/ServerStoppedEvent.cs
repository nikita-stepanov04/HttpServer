namespace HttpServerCore.Mediators
{
    public class ServerStoppedEvent : IEvent
    {
        public int Port { get; set; }

        public ServerStoppedEvent(int port) => Port = port;
    }
}

using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace HttpServerCore.Server
{
    public interface IServerState
    {
        bool IsConnectionKeepAliveDisabled();

        Task AcceptRequestAsync(
            TcpListener tcpListener,
            List<HttpServerClient> clients,
            IHandler handler,
            ILoggerFactory loggerFactory,
            ILogger logger);
    }

    public class HttpServerContext
    {
        private IServerState _state;

        public HttpServerContext(IServerState state)
        {
            _state = state;
        }

        public bool IsConnectionKeepAliveDisabled() => _state.IsConnectionKeepAliveDisabled();

        public Task HandleRequestAsync(TcpListener tcpListener, List<HttpServerClient> clients,
            IHandler handler, ILoggerFactory loggerFactory, ILogger logger) =>
                _state.AcceptRequestAsync(tcpListener, clients, handler, loggerFactory, logger);
    }

    public class SingleThreadedState : IServerState
    {
        public bool IsConnectionKeepAliveDisabled() => true;

        public async Task AcceptRequestAsync(TcpListener tcpListener, List<HttpServerClient> _,
            IHandler handler, ILoggerFactory loggerFactory, ILogger logger)
        {
            tcpListener.Start();
            logger.LogInformation("Server started in {p1} mode with address: {p2}", "Single thread", tcpListener.LocalEndpoint);

            ILogger clientLogger = loggerFactory.CreateLogger<HttpServerClient>();

            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();

                Guid connectionId = Guid.NewGuid();
                using (clientLogger.BeginConnectionScope(connectionId))
                {
                    logger.LogInformation("Connection: {p1} => {p2}", client.Client.RemoteEndPoint, client.Client.LocalEndPoint);

                    var clientCompletionSource = new TaskCompletionSource();

                    new HttpServerClient(client, clientLogger, handler,
                        isConnectionKeepAliveDisabled: IsConnectionKeepAliveDisabled(),
                        disposeCallback: httpClient => httpClient.Dispose(),
                        resultCallback: () =>
                        {
                            clientCompletionSource.SetResult();
                            return Task.CompletedTask;
                        }
                    );

                    await clientCompletionSource.Task;
                }
            }
        }
    }

    public class MultiThreadedState : IServerState
    {
        public bool IsConnectionKeepAliveDisabled() => false;

        public async Task AcceptRequestAsync(TcpListener tcpListener, List<HttpServerClient> clients,
            IHandler handler, ILoggerFactory loggerFactory, ILogger logger)
        {
            tcpListener.Start();
            logger.LogInformation("Server started in {p1} mode with address: {p2}", "Multi thread", tcpListener.LocalEndpoint);

            ILogger clientLogger = loggerFactory.CreateLogger<HttpServerClient>();

            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();

                Guid connectionId = Guid.NewGuid();
                using (clientLogger.BeginConnectionScope(connectionId))
                {
                    logger.LogInformation("Connection: {p1} => {p2}", client.Client.RemoteEndPoint, client.Client.LocalEndPoint);
                    lock (clients)
                    {
                        clients.Add(new HttpServerClient(client, clientLogger, handler,
                            isConnectionKeepAliveDisabled: IsConnectionKeepAliveDisabled(),
                            resultCallback: null,
                            disposeCallback: httpClient =>
                            {
                                lock (clients)
                                {
                                    clients.Remove(httpClient);
                                }
                                httpClient.Dispose();
                            }
                        ));
                    }
                }
            }
        }
    }
}

using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace HttpServerCore
{
    public class HttpServer : IDisposable
    {
        private readonly TcpListener _tcpListener;
        private readonly List<HttpServerClient> _httpServerClients = new();
        private readonly IHandler _handler;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly int _port;
        private readonly ProcessingMode _mode;

        public HttpServer(
            int port,
            ILoggerFactory loggerFactory,
            IHandler handler,
            ProcessingMode mode)
        {
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _logger = loggerFactory.CreateLogger<HttpServer>();
            _loggerFactory = loggerFactory;
            _handler = handler;
            _mode = mode;
        }

        public async Task StartAsync()
        {
            using (_logger.BeginServerScope(_port))
            {
                try
                {
                    _tcpListener.Start();
                    _logger.LogInformation("Server started with address: {p}", _tcpListener.LocalEndpoint);

                    ILogger clientLogger = _loggerFactory.CreateLogger<HttpServerClient>();

                    while (true)
                    {
                        TcpClient client = await _tcpListener.AcceptTcpClientAsync();

                        Guid connectionId = Guid.NewGuid();
                        using (clientLogger.BeginConnectionScope(connectionId))
                        {
                            _logger.LogInformation("Connection: {p1} => {p2}", client.Client.RemoteEndPoint, client.Client.LocalEndPoint);

                            lock (_httpServerClients)
                            {
                                _httpServerClients.Add(new HttpServerClient(client, clientLogger, _handler,
                                    disposeCallback: httpClient =>
                                    {
                                        lock (_httpServerClients)
                                        {
                                            _httpServerClients.Remove(httpClient);
                                        }
                                        httpClient.Dispose();
                                    }
                                ));
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    string message = e switch
                    {
                        SocketException => "Socket connection closed, stopping server",
                        ObjectDisposedException => "Server is already stopped, incoming connections are blocked",
                        _ => $"An unhandled exception occurred, message: {e.Message}"
                    };
                    _logger.LogInformation(message);
                }
            }
        }

        public void Stop() => _tcpListener.Stop();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;
        public virtual void Dispose(bool disposing)
        {
            if (disposed) throw new ObjectDisposedException(nameof(HttpServer));

            disposed = true;
            _tcpListener.Stop();

            if (disposing)
            {
                _logger.LogInformation("Stopping connected clients");
                lock (_httpServerClients)
                {
                    foreach (var client in _httpServerClients)
                    {
                        client.Dispose();
                    }
                }                
                _logger.LogInformation("Connected clients are stopped");
            }
            _logger.LogInformation("Server stopped successfully");
        }

        ~HttpServer() => Dispose(false);
    }
}

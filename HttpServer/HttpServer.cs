using Serilog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace HttpServerCore
{
    public class HttpServer : IDisposable
    {
        private readonly TcpListener _tcpListener;
        private readonly ConcurrentBag<HttpServerClient> _httpServerClients = new();
        private readonly ILogger _logger;

        public HttpServer(int port, ILogger logger)
        {
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _logger = logger;
        }

        public async Task StartAsync()
        {
            try
            {
                _tcpListener.Start();
                _logger.Information("Server started with address: {p}", _tcpListener.LocalEndpoint);

                while(true)
                {
                    TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                    _logger.Information("Connection: {p1} > {p2}", client.Client.RemoteEndPoint, client.Client.LocalEndPoint);
                    _httpServerClients.Add(new HttpServerClient());
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
                _logger.Information(message);
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
                _logger.Information("Stopping connected clients");
                foreach (var client in _httpServerClients)
                {
                    client.Dispose();
                }
                _logger.Information("Connected clients are stopped");
            }
            _logger.Information("Server stopped successfully");
        }

        ~HttpServer() => Dispose(false);
    }
}

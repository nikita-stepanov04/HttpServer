using HttpServerCore.Mediators;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace HttpServerCore
{
    public class HttpServer : IDisposable
    {
        private readonly TcpListener _tcpListener;
        private readonly List<HttpServerClient> _httpServerClients = new();
        private readonly HttpServerContext _context;
        private readonly IHandler _handler;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly int _port;
        private readonly Mediator _mediator;

        public HttpServer(
            int port,
            ILoggerFactory loggerFactory,
            IHandler handler,
            HttpServerContext context,
            Mediator mediator)
        {
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _logger = loggerFactory.CreateLogger<HttpServer>();
            _loggerFactory = loggerFactory;
            _handler = handler;
            _context = context;
            _mediator = mediator;
        }

        public async Task StartAsync()
        {
            using (_logger.BeginServerScope(_port))
            {
                try
                {
                    await _mediator.RaiseAsync(new ServerStartedEvent(_port));
                    await _context.HandleRequestAsync(_tcpListener, _httpServerClients, _handler, _loggerFactory, _logger);
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

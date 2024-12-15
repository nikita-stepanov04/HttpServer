using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using static HttpServerCore.NetworkHelpers;

namespace HttpServerCore
{
    public class HttpClient : IDisposable
    {
        private const string _serverIP = "127.0.0.1";
        private readonly IPAddress address = IPAddress.Parse(_serverIP);

        private readonly TcpClient _tcpClient;
        private readonly ILogger<HttpClient> _logger;
        private NetworkStream? _stream;

        public HttpClient(ILoggerFactory loggerFactory)
        {
            _tcpClient = new TcpClient();
            _logger = loggerFactory.CreateLogger<HttpClient>();
        }

        public async Task SendAsync(HttpRequest request, HttpResponse response, int port)
        {
            // Connecting

            if (_tcpClient.Connected) return;

            _logger.LogInformation("Establishing TCP connection with port: {p}", port);

            await _tcpClient.ConnectAsync(address, port);
            _stream = _tcpClient.GetStream();

            _logger.LogInformation("TCP connection was established");

            // Writing request 

            using (StreamWriter sw = new(_stream, leaveOpen: true))
            {
                sw.WriteLine($"{request.Method} {request.Uri}{request.QueryParams.ToString()} HTTP/1.1");
                foreach (var kvp in request.Headers)
                {
                    sw.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
                sw.WriteLine();
            }
            if (request.Content?.Length > 0)
                await request.Content.CopyToAsync(_stream);

            // Reading response 

            string requestLine = await ReadLineFromNetworkAsync(_stream);
            string[] lineTokens = requestLine.Split(" ");

            response.Protocol = lineTokens[0];
            response.StatusCode = (StatusCodes)Enum.Parse(typeof(StatusCodes), lineTokens[1]);

            await ParseHeaders(_stream, response.Headers);

            long length = response.Headers.GetDigit("Content-Length") ?? 0;
            if (length > 0)
            {
                await ReadRequestContentAsync(_stream, response.Content, length);
                response.Content.Position = 0;
            }
        }

        public void Dispose()
        {
            _tcpClient.Close();
            _stream?.Dispose();
            _tcpClient.Dispose();
        }
    }
}

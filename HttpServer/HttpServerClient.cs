using HttpServerCore.Handlers;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServerCore
{
    internal class HttpServerClient : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly Action<HttpServerClient> _disposeCallback;
        private readonly Task _clientTask;
        private readonly string _remoteEndpoint;
        private readonly IEndpointProvider _endpointProvider;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public HttpServerClient(
            TcpClient client,
            ILogger logger,
            IEndpointProvider endpointProvider,
            ILoggerFactory loggerFactory,
            Action<HttpServerClient> disposeCallback)
        {
            _client = client;
            _stream = client.GetStream();
            _disposeCallback = disposeCallback;
            _logger = logger;
            _endpointProvider = endpointProvider;
            _loggerFactory = loggerFactory;
            _remoteEndpoint = _client.Client.RemoteEndPoint?.ToString() ?? "";
            _clientTask = RunTcpReadingLoop();
        }

        private async Task RunTcpReadingLoop()
        {
            ILogger endpointLogger = _loggerFactory.CreateLogger<EndpointExecutor>();
            EndpointExecutor endpointExecutor = new(_endpointProvider, endpointLogger);

            try
            {
                while (true)
                {
                    (HttpRequest request, StatusCodes code) = await ReceiveHttpRequest();
                    using (request)
                    {
                        using HttpResponse response = new();
                        response.Protocol = request.Protocol ?? response.Protocol;
                        response.Headers.Set("Connection", request.Headers.Get("Connection") ?? "close");

                        if ((int)code >= 400)
                        {
                            response.StatusCode = code;
                            response.Headers.Set("Connection", "close");
                            await endpointExecutor.SelectErrorEndpointAndExecuteAsync(request, response);
                        }
                        else
                        {                            
                            await endpointExecutor.SelectEndpointAndExecuteAsync(request, response);
                        }

                        await SendHttpResponse(response);

                        if (response.Headers.Get("Connection") == "close")                        
                            break;                        
                    }
                }
                _logger.LogInformation("Connection to {p} was closed", _remoteEndpoint);                
            }
            catch (HttpRequestException)
            {
                _logger.LogInformation("Connection to {p} was terminated by client", _remoteEndpoint);
            }
            catch (IOException)
            {
                _logger.LogInformation("Connection to {p} was terminated by server", _remoteEndpoint);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.GetType().Name} happened while processing request: {e.Message}");
            } 
            finally
            {
                _stream.Close();
                if (!disposed)
                    _disposeCallback(this);
            }
        }

        private async Task<(HttpRequest, StatusCodes)> ReceiveHttpRequest()
        {
            HttpRequest request = new();

            // Parsing request line
            string requestLine = await ReadRequestLineAsync();
            string[] lineTokens = requestLine.Split(" ");

            if (lineTokens.Length != 3)
                return (request, StatusCodes.BadRequest);

            request.Method = lineTokens[0];            
            request.Uri = lineTokens[1];            

            // Check protocol version
            string[] protocolTokens = lineTokens[2].Split("/", 2);
            if (protocolTokens.Length != 2 || protocolTokens[1] != "1.1")
                return (request, StatusCodes.HttpVersionNotSupported);
            request.Protocol = lineTokens[2];

            // Parsing headers
            while (true)
            {
                string headerLine = await ReadRequestLineAsync();

                // End of headers
                if (headerLine.Length == 0)
                    break;

                string[] headerTokens = headerLine.Split(":", 2);
                if (headerTokens.Length == 2)
                {
                    request.Headers.Add(headerTokens[0], headerTokens[1]);
                }
            }

            //Check Uri
            if (request.Uri.Contains(".."))
                return (request, StatusCodes.BadRequest);

            // Parsing content
            long length = request.Headers.GetDigit("Content-Length") ?? 0;
            if (length > 0)
            {
                request.Content = new MemoryStream();
                await ReadRequestContentAsync(_stream, request.Content, length);
                request.Content.Position = 0;
            }

            return (request, StatusCodes.OK);
        }

        private async Task SendHttpResponse(HttpResponse response)
        { 
            using (StreamWriter sw = new(_stream, leaveOpen: true))
            {                
                sw.WriteLine($"{response.Protocol} {(int)response.StatusCode} {response.ReasonPhrase}");
                foreach (var kvp in response.Headers)
                {
                    sw.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
                sw.WriteLine();               
            }
            await response.Content.CopyToAsync(_stream);
        }

        private async Task<string> ReadRequestLineAsync() => await Task.Run(ReadRequestLine);

        private string ReadRequestLine()
        {
            LineState lineState = LineState.None;
            StringBuilder stringBuilder = new(128);

            while (true)
            {
                int b = _stream.ReadByte();
                lineState = b switch
                {
                    -1 => throw new HttpRequestException(),
                    '\r' when lineState == LineState.None => LineState.CR,
                    '\n' when lineState == LineState.CR => LineState.LF,
                    '\r' or '\n' => throw new ProtocolViolationException(),
                    _ => LineState.None
                };

                if (lineState == LineState.None)
                    stringBuilder.Append((char)b);
                else if (lineState == LineState.LF)
                    break;
            }
            return stringBuilder.ToString();
        }
        
        private async Task ReadRequestContentAsync(Stream source, Stream target, long count)
        {
            int bufferSize = 65536;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                while (count > 0)
                {
                    int bytesReceived = await source.ReadAsync(buffer, 0, (int)Math.Min(count, bufferSize));
                    if (bytesReceived == 0)
                        break;
                    await target.WriteAsync(buffer.AsMemory(0, bytesReceived));
                    count -= bytesReceived;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private enum LineState
        {
            None,
            LF,
            CR
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;
        public virtual void Dispose(bool disposing)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(HttpServerClient));
            disposed = true;
            if (_client.Connected)
            {
                _stream.Close();
                _clientTask.Wait();
            }
            if (disposing)
                _client.Dispose();
        }

        ~HttpServerClient() => Dispose(false);
    }
}
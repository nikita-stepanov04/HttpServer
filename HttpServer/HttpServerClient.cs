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
        private readonly ILogger _logger;

        public HttpServerClient(
            TcpClient client,
            ILogger logger,
            Action<HttpServerClient> disposeCallback)
        {
            _client = client;
            _stream = client.GetStream();
            _disposeCallback = disposeCallback;
            _logger = logger;
            _clientTask = ReceiveHttpRequest();
        }

        private async Task<(HttpRequest?, HttpStatusCode)> ReceiveHttpRequest()
        {
            HttpRequest request = new();

            // Parsing request line
            string requestLine = await ReadRequestLineAsync();
            string[] lineTokens = requestLine.Split(" ");

            if (lineTokens.Length != 3)
                return (null, HttpStatusCode.BadRequest);

            request.Method = lineTokens[0];
            request.Uri = lineTokens[1];
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

            // Parsing content
            long length = request.Headers.GetDigit("Content-Length") ?? 0;
            if (length > 0)
            {
                request.Content = new MemoryStream();
                await ReadRequestContentAsync(_stream, request.Content, length);
                request.Content.Position = 0;
            }

            return (request, HttpStatusCode.OK);
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
            throw new NotImplementedException();
        }
    }
}
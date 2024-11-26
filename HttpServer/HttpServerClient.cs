﻿using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServerCore
{
    public class HttpServerClient : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly Action<HttpServerClient> _disposeCallback;
        private readonly Func<Task>? _resultCallback;
        private readonly IHandler _handler;
        private readonly Task _clientTask;
        private readonly string _remoteEndpoint;
        private readonly ILogger _logger;
        private readonly bool _isConnectionKeepAliveDisabled;

        public HttpServerClient(
            TcpClient client,
            ILogger logger,
            IHandler handler,
            bool isConnectionKeepAliveDisabled,
            Func<Task>? resultCallback,
            Action<HttpServerClient> disposeCallback)
        {
            _client = client;
            _stream = client.GetStream();
            _resultCallback = resultCallback;
            _disposeCallback = disposeCallback;
            _logger = logger;
            _remoteEndpoint = _client.Client.RemoteEndPoint?.ToString() ?? "";
            _handler = handler;
            _isConnectionKeepAliveDisabled = isConnectionKeepAliveDisabled;
            _clientTask = RunTcpReadingLoop();
        }

        async Task RunTcpReadingLoop()
        {            
            try
            {
                while (true)
                {
                    (HttpRequest request, StatusCodes code) = await ReceiveHttpRequest();
                    Guid requestId = Guid.NewGuid();

                    using (request)
                    using (HttpResponse response = new())
                    using (_logger.BeginRequestScope(requestId))
                    {
                        request.RequestId = requestId;
                        response.RequestId = requestId;

                        response.Protocol = request.Protocol ?? response.Protocol;
                        response.Headers.Set("Connection", request.Headers.Get("Connection") ?? "close");

                        if ((int)code >= 400)
                        {
                            response.StatusCode = code;
                            response.Headers.Set("Connection", "close");
                        }
                        else
                        {
                            _logger.LogInformation("Request {p} was accepted successfully, invoking request handler", requestId);
                            await _handler.InvokeAsync(request, response);   
                        }                                                 

                        await SendHttpResponse(response);

                        if (_isConnectionKeepAliveDisabled || response.Headers.Get("Connection") == "close")                        
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
                _logger.LogError(e, "Error happened while processing request");
            } 
            finally
            {
                _stream.Close();
                if (!disposed)
                {
                    _disposeCallback(this);

                    if (_resultCallback != null)
                        await _resultCallback();
                }
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

            // Check Uri
            lineTokens[1] = Uri.UnescapeDataString(lineTokens[1]);
            if (lineTokens[1].Contains(".."))
                return (request, StatusCodes.BadRequest);

            // Parse uri and query params
            string[] uriTokens = lineTokens[1].Split('?', '&');
            request.Uri = uriTokens[0];
            for (int i = 1; i < uriTokens.Length; i++)
            {
                string[] queryTokens = uriTokens[i].Split('=', 2);
                if (queryTokens.Length == 2)
                {
                    request.QueryParams.Add(queryTokens[0], queryTokens[1]);
                }
            }

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
            response.Content.Position = 0;

            long? contentLength;
            if ((contentLength = response.Headers.GetDigit("Content-Length")) == null)
            {
                contentLength = response.Content.Length;
                response.Headers.Set("Content-Length", contentLength.Value.ToString());
            }            

            using (StreamWriter sw = new(_stream, leaveOpen: true))
            {                
                sw.WriteLine($"{response.Protocol} {(int)response.StatusCode} {response.ReasonPhrase}");
                foreach (var kvp in response.Headers)
                {
                    sw.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
                sw.WriteLine();               
            }
            if (contentLength > 0)
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
                    '\r' or '\n' => throw new ProtocolViolationException("Non supported protocol"),
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
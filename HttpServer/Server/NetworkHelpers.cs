using HttpServerCore.Request;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServerCore.Server
{
    internal static class NetworkHelpers
    {
        public async static Task ParseHeaders(NetworkStream stream, HeaderDictionary headers)
        {
            while (true)
            {
                string headerLine = await ReadLineFromNetworkAsync(stream);

                // End of headers
                if (headerLine.Length == 0)
                    break;

                string[] headerTokens = headerLine.Split(":", 2);
                if (headerTokens.Length == 2)
                {
                    if (!headers.ContainsKey(headerTokens[0]))
                        headers.Add(headerTokens[0], headerTokens[1]);
                }
            }
        }

        public async static Task ReadRequestContentAsync(Stream source, Stream target, long count)
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

        public static async Task<string> ReadLineFromNetworkAsync(NetworkStream stream) =>
            await Task.Run(() => ReadLineFromNetwork(stream));

        private static string ReadLineFromNetwork(NetworkStream stream)
        {
            LineState lineState = LineState.None;
            StringBuilder stringBuilder = new(128);

            while (true)
            {
                int b = stream.ReadByte();
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

        private enum LineState
        {
            None,
            LF,
            CR
        }
    }
}

using HttpServerCore;
using HttpServerCore.Request;
using System.Buffers;

namespace WebToolkit.ResponseWriting
{
    public class StaticResult : IEndpointResult
    {
        private readonly HttpResponse _httpResponse;
        private readonly string _filePath;
        private readonly int _bufferSize = 65536;

        public StaticResult(HttpResponse response, string filePath)
        {
            _httpResponse = response;
            _filePath = filePath;
        }

        public async Task ExecuteAsync()
        {
            using (FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int bytesRead = 0;
                int contentLength = 0;
                byte[] buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);

                try
                {
                    while ((bytesRead = await fs.ReadAsync(buffer, 0, _bufferSize)) > 0)
                    {
                        contentLength += bytesRead;
                        await _httpResponse.Content.WriteAsync(buffer, 0, bytesRead);
                    }

                    _httpResponse.StatusCode = StatusCodes.OK;

                    string fileExtension = _filePath.Split(".").Last();
                    _httpResponse.Headers.Set("Content-Type", ContentTypes.Parse(fileExtension));
                    _httpResponse.Headers.Set("Content-Length", contentLength.ToString());
                    _httpResponse.Headers.Set("Cache-Control", "public,max-age=3600");
                    _httpResponse.Headers.Set("Expires", DateTime.UtcNow.AddHours(1).ToString("R"));

                    await _httpResponse.Content.FlushAsync();
                    _httpResponse.Content.Position = 0;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}

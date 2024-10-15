using HttpServerCore;
using System.Buffers;
using System.Collections.Concurrent;

namespace WebToolkit.Handling
{
    public class EndpointProvider : IEndpointProvider
    {
        private string? _wwwrootPath;
        private string? _errorPath;

        private readonly ConcurrentDictionary<string, RequestDelegate> _getEndpoints = new();
        private readonly ConcurrentDictionary<string, RequestDelegate> _postEndpoints = new();

        public void MapStaticPath(string path) => _wwwrootPath = path;

        public void MapErrorPath(string path) => _errorPath = path;

        public void MapGet(string path, RequestDelegate endpoint) => _getEndpoints.TryAdd(path, endpoint);

        public void MapPost(string path, RequestDelegate endpoint) => _postEndpoints.TryAdd(path, endpoint);

        public RequestDelegate? GetGetEndpoint(string path) =>
            _getEndpoints.TryGetValue(path, out var endpoint) ? endpoint : null;

        public RequestDelegate? GetPostEndpoint(string path) =>
            _postEndpoints.TryGetValue(path, out var endpoint) ? endpoint : null;

        public RequestDelegate? GetErrorEndpoint(string path) => GetStaticEndpoint(_errorPath, path);

        public RequestDelegate? GetStaticEndpoint(string path) => GetStaticEndpoint(_wwwrootPath, path);

        private RequestDelegate? GetStaticEndpoint(string? basePath, string path)
        {
            if (path == "/" || path == string.Empty)
                path = "/index.html";

            string fullPath = $"{basePath}/{path}";
            if (File.Exists(fullPath))
            {
                RequestDelegate endpoint = async (context) =>
                {
                    using (FileStream fs = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        int bytesRead = 0;
                        int bufferSize = 65536;
                        int contentLength = 0;
                        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

                        try
                        {
                            while ((bytesRead = await fs.ReadAsync(buffer, 0, bufferSize)) > 0)
                            {
                                contentLength += bytesRead;
                                await context.Response.Content.WriteAsync(buffer, 0, bytesRead);
                            }

                            string fileExtension = path.Split(".").Last();
                            context.Response.Headers.Set("Content-Type", ContentTypes.Parse(fileExtension));
                            context.Response.Headers.Set("Content-Length", contentLength.ToString());
                            context.Response.Content.Position = 0;
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                };
                return endpoint;
            }
            return null;
        }
    }
}

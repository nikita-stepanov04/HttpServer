using System.Collections.Concurrent;
using WebToolkit.ResponseWriting;

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
                    await context.Response.HtmlResult(fullPath).ExecuteAsync();
                };
                return endpoint;
            }
            return null;
        }
    }
}

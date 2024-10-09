namespace HttpServerCore.Handlers
{
    public interface IEndpointProvider
    {
        public void MapErrorPath(string path);

        public void MapStaticPath(string directory);

        public void MapGet(string path, RequestDelegate endpoint);

        public void MapPost(string path, RequestDelegate endpoint);

        public RequestDelegate? GetErrorEndpoint(string path);

        public RequestDelegate? GetStaticEndpoint(string path);

        public RequestDelegate? GetGetEndpoint(string path);

        public RequestDelegate? GetPostEndpoint(string path);
    }

    public delegate Task RequestDelegate(HttpRequest request, HttpResponse response);
}

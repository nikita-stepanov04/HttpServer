using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.Handling;
using WebToolkit.Middleware;

namespace WebToolkit
{
    public class HttpServerBuilder
    {
        private readonly int _port;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMiddlewareProvider _middleware;
        private readonly IEndpointProvider _endpoints;
        private readonly ProcessingMode _mode;

        public HttpServerBuilder(int port, ILoggerFactory loggerFactory, ProcessingMode mode)
        {
            _port = port;
            _loggerFactory = loggerFactory;
            _middleware = new MiddlewareProvider();
            _endpoints = new EndpointProvider();
            _mode = mode;
        }

        public void Use(IMiddleware middleware) => _middleware.Use(middleware);

        public void Use<T>() where T : IMiddleware, new() => _middleware.Use(new T());

        public void UseEndpoints() => _middleware.Use(
            new EndpointExecutionMiddleware(_endpoints, _loggerFactory.CreateLogger<EndpointExecutionMiddleware>()));

        public void UseErrorMiddleware() => _middleware.Use(
            new ErrorMiddleware(_endpoints, _loggerFactory.CreateLogger<ErrorMiddleware>()));

        public void MapStaticPath(string path) => _endpoints.MapStaticPath(path);

        public void MapErrorPath(string path) => _endpoints.MapErrorPath(path);

        public void MapGet(string path, RequestDelegate endpoint) => _endpoints.MapGet(path, endpoint);

        public void MapPost(string path, RequestDelegate endpoint) => _endpoints.MapPost(path, endpoint);


        public HttpServer Build()
        {
            return new(_port, _loggerFactory, _middleware, _mode);
        }
    }
}

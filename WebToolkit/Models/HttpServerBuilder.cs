using HttpServerCore;
using HttpServerCore.Mediators;
using Microsoft.Extensions.Logging;
using WebToolkit.RequestHandling;
using WebToolkit.Middleware;
using WebToolkit.ResponseWriting;

namespace WebToolkit.Models
{
    public class HttpServerBuilder
    {
        private readonly int _port;
        private readonly ILoggerFactory _loggerFactory;
        private readonly MiddlewareComposite _middleware;
        private readonly IEndpointProvider _endpoints;
        private readonly ProcessingMode _mode;
        private readonly Mediator _mediator;

        public HttpServerBuilder(int port, ILoggerFactory loggerFactory, ProcessingMode mode)
        {
            _port = port;
            _loggerFactory = loggerFactory;
            _middleware = new MiddlewareComposite();
            _endpoints = new EndpointProvider();
            _mode = mode;
            _mediator = new(loggerFactory);
        }

        public void Use(IMiddleware middleware) => _middleware.Use(middleware);

        public void Use<T>() where T : IMiddleware, new() => _middleware.Use(new T());

        public void UseEndpoints() => _middleware.Use(
            new EndpointExecutionMiddleware(_endpoints, _loggerFactory.CreateLogger<EndpointExecutionMiddleware>()));

        public void UseErrorMiddleware() => _middleware.Use(
            new ErrorMiddleware(_endpoints, _loggerFactory.CreateLogger<ErrorMiddleware>()));

        public void MapViewsAssemblyType(Type assembly) => RazorHelper.ConfigureEngine(assembly);

        public void PrecompileViews() => RazorHelper.PrecompileViews().Wait();

        public void MapStaticPath(string path) => _endpoints.MapStaticPath(path);

        public void MapErrorPath(string path) => _endpoints.MapErrorPath(path);

        public void MapGet(string path, RequestDelegate endpoint) => _endpoints.MapGet(path, endpoint);

        public void MapPost(string path, RequestDelegate endpoint) => _endpoints.MapPost(path, endpoint);

        public void AddOnServerStartedEventHandler<T>() 
            where T : IEventHandler<ServerStartedEvent>, new() => _mediator.Register(new T());
        
        public void AddOnServerStoppedEventHandler<T>() 
            where T : IEventHandler<ServerStoppedEvent>, new() => _mediator.Register(new T());

        public HttpServer Build()
        {
            IServerState state = _mode switch
            {
                ProcessingMode.SingleThread => new SingleThreadedState(),
                ProcessingMode.MultiThread => new MultiThreadedState(),
                _ => throw new Exception("Selected server mode is not supported")
            };
            var context = new HttpServerContext(state);

            var middlewareProvider = new MiddlewareProvider(_middleware);

            return new(_port, _loggerFactory, middlewareProvider, context, _mediator);
        }
    }
}

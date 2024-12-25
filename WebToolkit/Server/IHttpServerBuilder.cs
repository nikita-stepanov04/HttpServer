using HttpServerCore.Mediators;
using HttpServerCore.Server;
using WebToolkit.RequestHandling;

namespace WebToolkit.Server
{
    public interface IHttpServerBuilder
    {
        public void Use(IMiddleware middleware);

        public void Use<T>() where T : IMiddleware, new();

        public void UseEndpoints();

        public void UseErrorMiddleware();

        public void MapViewsAssemblyType(Type assembly);

        public void PrecompileViews();

        public void MapStaticPath(string path);

        public void MapErrorPath(string path);

        public void MapGet(string path, RequestDelegate endpoint);

        public void MapPost(string path, RequestDelegate endpoint);

        public void AddOnServerStartedEventHandler<T>() where T : IEventHandler<ServerStartedEvent>, new();

        public void AddOnServerStoppedEventHandler<T>() where T : IEventHandler<ServerStoppedEvent>, new();

        public HttpServer Build();
    }
}

using DispatcherToolKit.Middleware;
using WebToolkit.Server;

namespace DispatcherToolKit
{
    public static class HttpServerBuilderExtensions
    {
        public static void UseRequestForwarding(this IHttpServerBuilder builder)
        {
            builder.Use(new RequestForwardingMiddleware());
        }

        public static void MapDispatcherEndpoints(this IHttpServerBuilder builder)
        {
            builder.MapGet("/register", DispatcherEndpoints.RegisterServer);
            builder.MapGet("/unregister", DispatcherEndpoints.UnregisterServer);
            builder.MapGet("/get", DispatcherEndpoints.GetAddress);
        }
    }
}

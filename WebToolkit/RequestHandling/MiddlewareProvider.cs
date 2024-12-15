using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.Models;

namespace WebToolkit.RequestHandling
{
    public class MiddlewareProvider : IHandler
    {
        private readonly IMiddleware _middleware;
        private readonly ILoggerFactory _loggerFactory;

        public MiddlewareProvider(IMiddleware middleware, ILoggerFactory loggerFactory)
        {
            _middleware = middleware;
            _loggerFactory = loggerFactory;
        }

        public async Task InvokeAsync(HttpRequest request, HttpResponse response)
        {
            var context = new HttpContext(request, response, _loggerFactory);

            await _middleware.InvokeAsync(context, () => Task.CompletedTask);            
        }
    }
}

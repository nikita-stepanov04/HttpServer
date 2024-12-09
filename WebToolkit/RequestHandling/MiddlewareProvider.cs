using HttpServerCore;
using WebToolkit.Models;

namespace WebToolkit.RequestHandling
{
    public class MiddlewareProvider : IHandler
    {
        private readonly IMiddleware _middleware;

        public MiddlewareProvider(IMiddleware middleware)
        {
            _middleware = middleware;
        }

        public async Task InvokeAsync(HttpRequest request, HttpResponse response)
        {
            var context = new HttpContext(request, response);

            await _middleware.InvokeAsync(context, () => Task.CompletedTask);            
        }
    }
}

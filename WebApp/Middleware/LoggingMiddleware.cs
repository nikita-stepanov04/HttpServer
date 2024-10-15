using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.Handling;

namespace WebApp.Middleware
{
    public class LoggingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;

        public Task InvokeAsync(HttpRequest request, HttpResponse response, Func<Task> Next)
        {
            throw new NotImplementedException();
        }
    }
}

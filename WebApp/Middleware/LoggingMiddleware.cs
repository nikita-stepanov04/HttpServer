using Microsoft.Extensions.Logging;
using WebToolkit.RequestHandling;
using WebToolkit.Models;

namespace WebApp.Middleware
{
    public class LoggingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;

        public Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            throw new NotImplementedException();
        }
    }
}

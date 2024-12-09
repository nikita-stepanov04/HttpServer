using Microsoft.Extensions.Logging;
using WebToolkit.RequestHandling;
using WebToolkit.Models;

namespace DispatcherToolKit.Middleware
{
    public class RequestForwardingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly HttpServerCore.HttpClient _httpClient;

        public Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            throw new NotImplementedException();
        }
    }
}

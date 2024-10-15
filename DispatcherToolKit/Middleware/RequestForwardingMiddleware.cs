using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.Handling;

namespace DispatcherToolKit.Middleware
{
    public class RequestForwardingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly HttpServerCore.HttpClient _httpClient;

        public Task InvokeAsync(HttpRequest request, HttpResponse response, Func<Task> Next)
        {
            throw new NotImplementedException();
        }
    }
}

using HttpServerCore.Request;
using Microsoft.Extensions.Logging;

namespace WebToolkit
{
    public class HttpContext
    {
        public HttpRequest Request { get; private set; } = null!;
        public HttpResponse Response { get; private set; } = null!;
        public ILoggerFactory LoggerFactory { get; private set; }

        public HttpContext(HttpRequest request, HttpResponse response, ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            Request = request;
            Response = response;
        }
    }
}

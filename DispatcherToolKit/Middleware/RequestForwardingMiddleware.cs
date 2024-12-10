using Microsoft.Extensions.Logging;
using WebToolkit.RequestHandling;
using WebToolkit.Models;
using HttpServerCore;
using DispatcherToolKit.Handlers;
using System.Text.Json;

namespace DispatcherToolKit.Middleware
{
    public class RequestForwardingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public RequestForwardingMiddleware(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<RequestForwardingMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            bool? forward = context.Request.QueryParams.GetBool("forward");
            if (forward.HasValue && forward.Value)
            {
                context.Request.QueryParams.Remove("forward");

                var client = new HttpServerCore.HttpClient(_loggerFactory);

                // Requesting dispatcher
                var request = new HttpRequest()
                {
                    Method = "GET",
                    Uri = "/get"
                };
                await request.WriteJsonAsync(context.Request.ServerPort);
                var response = new HttpResponse();

                _logger.LogInformation("Requesting dispatcher");

                await client.SendAsync(request, response, DispatcherConnection.DispatcherPort);

                if ((int)response.StatusCode >= 400) throw new Exception();

                using var reader = new StreamReader(response.Content);
                string responseJson = reader.ReadToEnd();
                int? forwardPort = JsonSerializer.Deserialize<int?>(responseJson);

                if (!forwardPort.HasValue) throw new Exception();

                _logger.LogInformation("Successfully received server address from dispatcher");

                // Requesting server

                _logger.LogInformation("Forwarding request to server: {p}", forwardPort);
                client = new HttpServerCore.HttpClient(_loggerFactory);
                await client.SendAsync(context.Request, context.Response, forwardPort.Value);
            }
            else
                await next();
        }
    }
}

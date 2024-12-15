using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.RequestHandling;
using WebToolkit.Models;
using System.Diagnostics;

namespace WebToolkit.Middleware
{
    public class EndpointExecutionMiddleware : IMiddleware
    {
        private readonly IEndpointProvider _endpointProvider;
        private readonly ILogger _logger;

        public EndpointExecutionMiddleware(IEndpointProvider endpointProvider, ILogger logger)
        {
            _endpointProvider = endpointProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Uri;

            RequestDelegate? endpoint = _endpointProvider.GetStaticEndpoint(path)
                ?? request.Method switch
                {
                    "GET" => _endpointProvider.GetGetEndpoint(path),
                    "POST" => _endpointProvider.GetPostEndpoint(path),
                    _ => null
                };

            if (endpoint == null)
            {
                response.StatusCode = StatusCodes.NotFound;
                throw new EndpointNotFoundException("Endpoint was not found");
            }
            else
            {
                _logger.LogInformation("Endpoint for {p1} {p2} was found, executing", request.Method, path);
                await endpoint.Invoke(context);
                _logger.LogInformation("Endpoint for {p1} {p2} was executed successfully", request.Method, path);
            }
            await next();
        }
    }
}

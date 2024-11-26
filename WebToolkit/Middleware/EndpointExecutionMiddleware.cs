using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.Handling;
using WebToolkit.Models;

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

        public async Task InvokeAsync(HttpContext context, Func<Task> Next)
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
                _logger.LogError("Endpoint for {p1} {p2} was not found", request.Method, path);
            }
            else
            {
                try
                {
                    _logger.LogInformation("Endpoint for {p1} {p2} was found, executing", request.Method, path);
                    await endpoint.Invoke(context);

                    _logger.LogInformation("Endpoint was executed successfully");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to execute endpoint due to endpoint exception");                    
                    response.StatusCode = StatusCodes.InternalServerError;
                }
            }
            await Next();
        }
    }
}

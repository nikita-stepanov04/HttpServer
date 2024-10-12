using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.Handling;

namespace WebToolkit.Middleware
{
    internal class ErrorMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly IEndpointProvider _endpointProvider;

        public ErrorMiddleware(IEndpointProvider endpointProvider, ILogger logger)
        {
            _logger = logger;
            _endpointProvider = endpointProvider;
        }

        public async Task InvokeAsync(HttpRequest request, HttpResponse response, Func<Task> Next)
        {
            if ((int)response.StatusCode >= 400)
            {
                response.Content.SetLength(0);
                response.Content.Position = 0;

                _logger.LogInformation("Searching for {p} error endpoint", (int)response.StatusCode);

                string path = $"{(int)response.StatusCode}.html";
                RequestDelegate? errorEndpoint = _endpointProvider.GetErrorEndpoint(path);

                if (errorEndpoint == null)
                {
                    _logger.LogError("Failed to find error endpoint");
                    return;
                }

                _logger.LogInformation("Error endpoint was found successfully");

                await errorEndpoint.Invoke(request, response);
                response.StatusCode = StatusCodes.OK;

                _logger.LogInformation("Successfully executed error endpoint");
            }
            else
                await Next();
        }
    }
}

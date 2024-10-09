using Microsoft.Extensions.Logging;

namespace HttpServerCore.Handlers
{
    internal class EndpointExecutor
    {
        private readonly IEndpointProvider _endpointProvider;
        private readonly ILogger _logger;

        public EndpointExecutor(IEndpointProvider endpointProvider, ILogger logger)
        {
            _endpointProvider = endpointProvider;
            _logger = logger;
        }

        public async Task<bool> SelectEndpointAndExecuteAsync(HttpRequest request, HttpResponse response)
        {
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
                    await endpoint.Invoke(request, response);
                    response.StatusCode = StatusCodes.OK;

                    _logger.LogInformation("Endpoint was executed successfully");
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to execute endpoint due to endpoint exception: {e.Message}");
                    response.StatusCode = StatusCodes.InternalServerError;                    
                }
            }

            return await SelectErrorEndpointAndExecuteAsync(request, response);
        }

        public async Task<bool> SelectErrorEndpointAndExecuteAsync(HttpRequest request, HttpResponse response)
        {
            response.Content.SetLength(0);
            response.Content.Position = 0;

            _logger.LogInformation("Searching for {p} error endpoint", (int)response.StatusCode);

            string path = $"{(int)response.StatusCode}.html";
            RequestDelegate? errorEndpoint = _endpointProvider.GetErrorEndpoint(path);

            if (errorEndpoint == null)
            {
                _logger.LogError("Failed to find error endpoint");
                return false;
            }

            _logger.LogInformation("Error endpoint was found successfully");

            await errorEndpoint.Invoke(request, response);
            response.StatusCode = StatusCodes.OK;

            _logger.LogInformation("Successfully executed error endpoint");

            return true;
        }
    }
}

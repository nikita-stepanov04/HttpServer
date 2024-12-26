using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.RequestHandling;

namespace WebToolkit.Middleware
{
    public class ErrorMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly IEndpointProvider _endpointProvider;
        private readonly bool _useErrorPages;

        public ErrorMiddleware(IEndpointProvider endpointProvider, 
            ILogger logger, bool useErrorPages = false)
        {
            _logger = logger;
            _endpointProvider = endpointProvider;
            _useErrorPages = useErrorPages;
        }

        public async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (Exception e)
            {
                _logger.LogError($"Request wasn't processed successfully: {e.Message}");

                var response = context.Response;
                response.Content.SetLength(0);
                response.Content.Position = 0;

                response.StatusCode = (int)response.StatusCode >= 400
                    ? response.StatusCode
                    : StatusCodes.InternalServerError;

                if (_useErrorPages)
                {
                    _logger.LogInformation("Searching for {p} error endpoint", (int)response.StatusCode);

                    string path = $"{(int)response.StatusCode}.html";
                    RequestDelegate? errorEndpoint = _endpointProvider.GetErrorEndpoint(path);

                    if (errorEndpoint == null)
                    {
                        _logger.LogError("Failed to find error endpoint");
                        return;
                    }

                    _logger.LogInformation("Error endpoint was found successfully");

                    await errorEndpoint.Invoke(context);
                    response.StatusCode = StatusCodes.OK;

                    _logger.LogInformation("Successfully executed error endpoint");
                }
            }
        }
    }
}

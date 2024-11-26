using HttpServerCore;

namespace WebToolkit.ResponseWriting
{
    public class HttpStatusResult : IEndpointResult
    {
        private readonly StatusCodes _statusCode;
        private readonly HttpResponse _httpResponse;

        public HttpStatusResult(HttpResponse response, StatusCodes statusCode)
        {
            _statusCode = statusCode;
            _httpResponse = response;
        }

        public Task ExecuteAsync()
        {
            _httpResponse.StatusCode = _statusCode;
            _httpResponse.Headers.Set("Content-Length", "0");
            return Task.CompletedTask;
        }
    }
}

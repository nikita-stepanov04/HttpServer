using HttpServerCore;
using System;
using System.Text.Json;

namespace WebToolkit.ResponseWriting
{
    public class JsonResult<T> : IEndpointResult
    {
        private readonly HttpResponse _httpResponse;
        private readonly T _entity;

        public JsonResult(HttpResponse response, T entity) 
        {
            _httpResponse = response;
            _entity = entity;
        }

        public async Task ExecuteAsync()
        {
            var content = _httpResponse.Content;
            await JsonSerializer.SerializeAsync(content, _entity);

            _httpResponse.Headers.Set("Content-Type", "application/json");
            _httpResponse.Headers.Set("Content-Length", content.Length.ToString());
            _httpResponse.StatusCode = StatusCodes.OK;

            await content.FlushAsync();
            content.Position = 0;
        }
    }
}

using System.Text.Json;

namespace HttpServerCore
{
    public class HttpRequest : IDisposable
    {
        public Guid RequestId { get; set; }

        public int ServerPort { get; set; }

        public string? Method { get; set; }
        public string Uri { get; set; } = "/";
        public string? Protocol { get; set; } = "HTTP/1.1";

        public QueryDictionary QueryParams { get; set; } = new();
        public HeaderDictionary Headers { get; set; } = new();
        public Stream? Content { get; set; }

        public void Dispose() => Content?.Dispose();

        public async Task WriteJsonAsync<T>(T entity)
        {
            await JsonSerializer.SerializeAsync(Content ??= new MemoryStream(), entity);
            Headers.Add("Content-Length", Content.Length.ToString());
            Content.Position = 0;
        }

        public async Task<T?> ReadJsonAsync<T>()
        {
            if (Content == null) return default;
            return await JsonSerializer.DeserializeAsync<T>(Content);
        }
    }
}

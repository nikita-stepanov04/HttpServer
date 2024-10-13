namespace HttpServerCore
{
    public class HttpRequest : IDisposable
    {
        public Guid RequestId { get; set; }

        public string? Method { get; set; }
        public string Uri { get; set; } = "/";
        public string? Protocol { get; set; }

        public QueryDictionary QueryParams { get; set; } = new();
        public HeaderDictionary Headers { get; set; } = new();
        public Stream? Content { get; set; }

        public void Dispose() => Content?.Dispose();
    }
}

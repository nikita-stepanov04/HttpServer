namespace HttpServerCore
{
    public class HttpRequest
    {
        public string? Method { get; set; }
        public string? Uri { get; set; }
        public string? Protocol { get; set; }
        public HeaderDictionary Headers { get; set; } = new();
        public Stream? Content { get; set; }
    }
}

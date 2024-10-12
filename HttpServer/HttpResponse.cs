namespace HttpServerCore
{
    public class HttpResponse : IDisposable
    {
        public string Protocol { get; set; } = "HTTP/1.1";

        public StatusCodes StatusCode { get; set; } = StatusCodes.NoContent;

        public string? ReasonPhrase => Enum.GetName(StatusCode);

        public HeaderDictionary Headers { get; set; } = new();

        public Stream Content { get; set; } = new MemoryStream();

        public void Dispose() => Content?.Dispose();
    }
}

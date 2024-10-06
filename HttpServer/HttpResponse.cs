using System.Net;

namespace HttpServerCore
{
    public class HttpResponse
    {
        public string? Protocol { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public HeaderDictionary Headers { get; set; } = new();
        public Stream? Content { get; set; }
    }
}

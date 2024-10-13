using Microsoft.Extensions.Logging;

namespace WebApp.Models
{
    public class ServerLog
    {
        public string? Message { get; set; }
        public LogLevel LogLevel { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? Exception { get; set; }
        public string? SourceType { get; set; }
        public List<Dictionary<string, string>>? Scope { get; set; }

        public HttpRequestLog? HttpRequest { get; set; }
        public HttpResponseLog? HttpResponse { get; set; }
    }
}

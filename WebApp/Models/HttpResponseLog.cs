using HttpServerCore;

namespace WebApp.Models
{
    public class HttpResponseLog
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public StatusCodes StatusCode { get; set; }
        public string? Headers { get; set; }
        public string? StringContent { get; set; }
        public byte[]? Content { get; set; }
    }
}

namespace WebApp.Models
{
    public class HttpRequestLog
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public string? Method { get; set; }
        public string? Uri { get; set; }
        public string? QueryParams { get; set; }
        public string? Headers { get; set; }
        public string? StringContent { get; set; }
        public byte[]? Content { get; set; }
    }
}

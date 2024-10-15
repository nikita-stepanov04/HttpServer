namespace HttpServerCore
{
    public class HttpClient
    {
        private readonly int _port;

        public HttpClient(int port)
        {
            _port = port;
        }

        public async Task<HttpResponse> GetAsync(HttpRequest request, int port)
        {
            throw new NotImplementedException();
        }
    }
}

using HttpServerCore;

namespace WebToolkit.Handling
{
    public class MiddlewareProvider : IMiddlewareProvider
    {
        private readonly List<IMiddleware> _middleware = new();

        public void Use(IMiddleware middleware) => _middleware.Add(middleware);

        public async Task InvokeAsync(HttpRequest request, HttpResponse response)
        {
            int index = 0;

            async Task Next()
            {
                IMiddleware? middleware = null;
                lock (_middleware)
                {
                    if (index < _middleware.Count)
                    {
                        middleware = _middleware[index++];
                    }
                }
                if (middleware != null)
                    await middleware.InvokeAsync(request, response, Next);
            }
            await Next();
        }
    }
}

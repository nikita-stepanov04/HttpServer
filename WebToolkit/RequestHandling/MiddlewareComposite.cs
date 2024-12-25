namespace WebToolkit.RequestHandling
{
    public class MiddlewareComposite : IMiddleware
    {
        private readonly List<IMiddleware> _middleware = new();

        public void Use(IMiddleware middleware) => _middleware.Add(middleware);

        public async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            int index = 0;

            next = async () =>
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
                    await middleware.InvokeAsync(context, next);
            };
            await next();
        }
    }
}

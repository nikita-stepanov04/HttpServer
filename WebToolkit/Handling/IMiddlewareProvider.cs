using HttpServerCore;
using Microsoft.Extensions.Logging;

namespace WebToolkit.Handling
{
    public interface IMiddlewareProvider : IHandler
    {
        void Use(IMiddleware middleware);
    }
}

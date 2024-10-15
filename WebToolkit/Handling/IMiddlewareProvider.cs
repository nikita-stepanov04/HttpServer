using HttpServerCore;

namespace WebToolkit.Handling
{
    public interface IMiddlewareProvider : IHandler
    {
        void Use(IMiddleware middleware);
    }
}

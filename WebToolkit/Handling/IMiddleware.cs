using HttpServerCore;

namespace WebToolkit.Handling
{
    public interface IMiddleware
    {
        Task InvokeAsync(HttpRequest request, HttpResponse response, Func<Task> Next);
    }
}

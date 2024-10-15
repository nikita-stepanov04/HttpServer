using WebToolkit.Models;

namespace WebToolkit.Handling
{
    public interface IMiddleware
    {
        Task InvokeAsync(HttpContext context, Func<Task> Next);
    }
}

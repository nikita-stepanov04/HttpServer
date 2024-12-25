namespace WebToolkit.RequestHandling
{
    public interface IMiddleware
    {
        Task InvokeAsync(HttpContext context, Func<Task> next);
    }
}

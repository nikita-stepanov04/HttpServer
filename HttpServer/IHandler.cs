namespace HttpServerCore
{
    public interface IHandler
    {
        Task InvokeAsync(HttpRequest request, HttpResponse response);
    }
}

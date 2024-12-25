using HttpServerCore.Request;

namespace HttpServerCore.Server
{
    public interface IHandler
    {
        Task InvokeAsync(HttpRequest request, HttpResponse response);
    }
}

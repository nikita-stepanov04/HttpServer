using WebApp.Models;
using WebToolkit.RequestHandling;
using WebToolkit.Models;

namespace WebApp.Middleware
{
    public class AuthMiddleware : IMiddleware
    {
        private readonly UserStore _userStore;

        public Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            throw new NotImplementedException();
        }
    }
}

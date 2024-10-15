using WebApp.Models;
using WebToolkit.Handling;
using WebToolkit.Models;

namespace WebApp.Middleware
{
    public class AuthMiddleware : IMiddleware
    {
        private readonly UserStore _userStore;

        public Task InvokeAsync(HttpContext context, Func<Task> Next)
        {
            throw new NotImplementedException();
        }
    }
}

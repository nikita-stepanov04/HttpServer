using HttpServerCore;
using WebApp.Models;

namespace WebToolkit.Models
{
    public class HttpContext
    {
        public HttpRequest Request { get; private set; } = null!;
        public HttpResponse Response { get; private set; } = null!;
        public bool IsAuthorized { get; set; }
        public UserRole UserRole { get; set; } = UserRole.None;
        public User? User { get; set; }

        public HttpContext(HttpRequest request, HttpResponse response)
        {
            Request = request;
            Response = response;
        }
    }
}

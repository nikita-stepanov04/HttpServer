using HttpServerCore;
using WebToolkit.Models;
using WebToolkit.ResponseWriting;
using static WebApp.WebAppHelper;

namespace WebApp.Endpoints
{
    public static class Endpoints
    {
        public static async Task TestHtml(HttpContext context)
        {
            await context.Response
                .StaticResult(ContentPath + "/index.html")
                .ExecuteAsync();
        }

        public static async Task TestJson(HttpContext context)
        {
            await context.Response
                .JsonResult(new { Test1 = "test1", Test2 = "test2" })
                .ExecuteAsync();
        }

        public static async Task TestRazor(HttpContext context)
        {
            await context.Response
                .RazorResult(viewModel: context, viewName: "Demo")
                .ExecuteAsync();
        }

        public static async Task TestStatus(HttpContext context)
        {
            await context.Response
                .HttpStatusResult(StatusCodes.MethodNotAllowed)
                .ExecuteAsync();
        }

        public static async Task FileLength(HttpContext context)
        {
            if (context.Request.Content != null)
            {
                long count = context.Request.Content.Length;
                await context.Response
                    .JsonResult(count)
                    .ExecuteAsync();
            }
        }
    }
}

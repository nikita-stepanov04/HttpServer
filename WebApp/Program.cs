using Configuration;
using DispatcherToolKit.Handlers;
using DispatcherToolKit.Middleware;
using HttpServerCore;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using WebApp.Models;
using WebToolkit.Models;
using WebToolkit.RequestHandling;
using WebToolkit.ResponseWriting;

namespace WebApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int port = 0;
            if (args.Length > 0)
            {
                port = Convert.ToInt32(args[0]);
                DispatcherConnection.DispatcherPort = Convert.ToInt32(args[1]);
            }
            else
            {
                port = 8080;
            }

            string contentPath = AppContext.BaseDirectory + "wwwroot";

            ILoggerFactory loggerFactory = new SerilogLoggerFactory(CommonSettingsConfiguration.Logger);
            HttpServerBuilder app = new(port, loggerFactory, ProcessingMode.MultiThread);

            //app.Use<Middleware1>();
            //app.Use<Middleware2>();
            app.UseErrorMiddleware();
            app.Use(new RequestForwardingMiddleware(loggerFactory));
            app.UseEndpoints();

            app.MapViewsAssemblyType(typeof(Program));
            app.PrecompileViews();

            app.MapStaticPath(contentPath);
            app.MapErrorPath(contentPath + "/errors");

            app.MapGet("/test-html", Endpoints.TestHtml);
            app.MapGet("/test-json", Endpoints.TestJson);
            app.MapGet("/test-razor", Endpoints.TestRazor);
            app.MapGet("/test-status", Endpoints.TestStatus);

            app.MapGet("/error", _ => throw new Exception());
            app.MapPost("/fileLength", Endpoints.FileLength);

            app.AddOnServerStartedEventHandler<ServerStartedEventHandler>();
            app.AddOnServerStoppedEventHandler<ServerStoppedEventHandler>();

            using HttpServer server = app.Build();

            await server.StartAsync();
        }
    }

    class Middleware1 : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            Console.WriteLine("Middleware 1");
            await next();
            Console.WriteLine("After all");
        }
    }

    class Middleware2 : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            Console.WriteLine("Middleware 2");
            await next();
        }
    }

    static class Endpoints
    {
        static string contentPath = AppContext.BaseDirectory + "wwwroot";

        public static async Task TestHtml(HttpContext context)
        {
            await context.Response
                .HtmlResult(contentPath + "/index.html")
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
            var user = new AppUser
            {
                UserId = Guid.NewGuid(),
                FirstName = "Test user name",
                SecondName = "Test user second name",
                Birthdate = DateOnly.Parse("2003/03/15")
            };
            await context.Response.RazorResult(user, viewName: "User").ExecuteAsync();
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
                using var sw = new StreamWriter(context.Response.Content, leaveOpen: true);

                string result = $"File length is: {count} bytes";

                context.Response.Headers.Set("Content-Type", "text/plain");

                await sw.WriteLineAsync(result);
                await sw.FlushAsync();
            }
        }
    }
}

using HttpServerCore;
using Serilog;
using Serilog.Extensions.Logging;
using WebToolkit.RequestHandling;
using System.Text;
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL;
using WebToolkit.Models;
using WebToolkit.ResponseWriting;
using WebApp.Models;
using WebToolkit.Handlers;

namespace WebApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            #region Configuration

            string contentPath = AppContext.BaseDirectory + "wwwroot";

            string connectionString = "Host=localhost;Database=HttpServer;Username=postgres;Password=Password123$";
            
            IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"timestamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
                {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                {"source_context", new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.ToString, NpgsqlDbType.Text) },
                {"scope", new SinglePropertyColumnWriter("Scope", PropertyWriteMethod.Json, NpgsqlDbType.Jsonb) }
            };

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.PostgreSQL(
                    connectionString: connectionString,
                    tableName: "ServerLogs",
                    columnOptions: columnWriters,
                    needAutoCreateTable: true)
                .CreateLogger();

            #endregion

            HttpServerBuilder app = new(8080, new SerilogLoggerFactory(), ProcessingMode.MultiThread);

            app.Use<Middleware1>();
            app.Use<Middleware2>();
            app.UseEndpoints();
            app.UseErrorMiddleware();

            app.MapViewsAssemblyType(typeof(Program));
            app.MapStaticPath(contentPath);
            app.MapErrorPath(contentPath + "/errors");

            app.MapGet("/test", Endpoints.Test);
            app.MapGet("/test-html", Endpoints.TestHtml);
            app.MapGet("/test-json", Endpoints.TestJson);
            app.MapGet("/test-razor", Endpoints.TestRazor);
            app.MapGet("/test-status", Endpoints.TestStatus);

            app.MapGet("/error", _ => throw new Exception());
            app.MapPost("/fileLength", Endpoints.FileLength);

            app.AddOnServerStartedEventHandler<ServerStartedEventHandler>();

            using HttpServer server = app.Build();
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    server.Stop();
                };
                await server.StartAsync();
            }            
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

        public static async Task Test(HttpContext context)
        {
            using var sw = new StreamWriter(context.Response.Content, leaveOpen: true);

            StringBuilder builder = new("Query params: ");
            foreach (var param in context.Request.QueryParams)
            {
                builder.Append($"{param.Key}: {param.Value}, ");
            }
            string result = builder.ToString();

            context.Response.Headers.Set("Content-Type", "text/plain");
            await sw.WriteAsync(result);
            await sw.FlushAsync();
        }
        
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

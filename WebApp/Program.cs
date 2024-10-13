using HttpServerCore;
using Serilog;
using Serilog.Extensions.Logging;
using WebToolkit.Handling;
using System.Text;
using WebToolkit;
using Npgsql;
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL;
using System.Runtime.CompilerServices;

namespace WebApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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

            HttpServerBuilder app = new(8080, new SerilogLoggerFactory());            

            app.Use<Middleware1>();
            app.Use<Middleware2>();
            app.UseEndpoints();
            app.UseErrorMiddleware();

            app.MapStaticPath(contentPath);
            app.MapErrorPath(contentPath + "/errors");

            app.MapGet("/test", Endpoints.Test);
            app.MapGet("/error", (rq, rs) => throw new Exception());
            app.MapPost("/fileLength", Endpoints.FileLength);

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
        public async Task InvokeAsync(HttpRequest request, HttpResponse response, Func<Task> Next)
        {
            Console.WriteLine("Middleware 1");
            await Next();
            Console.WriteLine("After all");
        }
    }

    class Middleware2 : IMiddleware
    {
        public async Task InvokeAsync(HttpRequest request, HttpResponse response, Func<Task> Next)
        {
            Console.WriteLine("Middleware 2");
            await Next();
        }
    }

    static class Endpoints
    {
        public static async Task Test(HttpRequest request, HttpResponse response)
        {
            using var sw = new StreamWriter(response.Content, leaveOpen: true);

            StringBuilder builder = new("Query params: ");
            foreach (var param in request.QueryParams)
            {
                builder.Append($"{param.Key}: {param.Value}, ");
            }
            string result = builder.ToString();

            response.Headers.Set("Content-Type", "text/plain");
            await sw.WriteAsync(result);
            await sw.FlushAsync();
        }

        public static async Task FileLength(HttpRequest request, HttpResponse response)
        {
            if (request.Content != null)
            {
                long count = request.Content.Length;
                using var sw = new StreamWriter(response.Content, leaveOpen: true);

                string result = $"File length is: {count} bytes";

                response.Headers.Set("Content-Type", "text/plain");

                await sw.WriteLineAsync(result);
                await sw.FlushAsync();
            }
        }
    }
}

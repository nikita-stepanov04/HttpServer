using HttpServerCore;
using System.Reflection.Metadata;
using System.Text.Json;
using WebToolkit;
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

        public static async Task Statistics(HttpContext context)
        {
            string statsFilePath = HttpServerCore.Statistics.StatisticsFilePath;
            Dictionary<string, RequestStatistics>? stats = null;
            try
            {
                string fileContent = await ReadFileWithRetriesAsync(statsFilePath);
                stats = JsonSerializer.Deserialize<Dictionary<string, RequestStatistics>>(fileContent);
            }
            catch {}
            await context.Response
                .RazorResult(viewModel: stats, viewName: "Stats")
                .ExecuteAsync();
        }

        private static async Task<string> ReadFileWithRetriesAsync(string filePath, int maxRetries = 5, int delayMilliseconds = 100)
        {
            int attempt = 0;

            while (true)
            {
                try
                {
                    return await File.ReadAllTextAsync(filePath);
                }
                catch (IOException)
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        throw;
                    }
                    await Task.Delay(delayMilliseconds);
                }
            }
        }

    }
}

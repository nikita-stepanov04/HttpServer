using HttpServerCore.Request;
using System.Collections.Concurrent;
using System.Text.Json;

namespace HttpServerCore
{
    public record RequestStatistics(int TotalRequests, int ErrorRequests, int SuccessRequests, long TotalTime);

    public static class Statistics
    {
        private const string AppName = "Http Server";
        private const string StatisticsFileName = "stats.json";

        private static readonly ConcurrentDictionary<string, RequestStatistics> _statistics = new();
        private static readonly Random _rand = new Random();

        private static Timer _timer;

        public static string StatisticsFilePath;

        static Statistics()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), AppName);
            Directory.CreateDirectory(folderPath);
            StatisticsFilePath = Path.Combine(folderPath, StatisticsFileName);

            _timer = new Timer(async _ => await WriteStatsToFileAsync(), null,
                TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public static void Log(HttpRequest request, HttpResponse response, long responseTime)
        {
            bool errorRequest = (int)response.StatusCode >= 400;
            var stat = new RequestStatistics(
                TotalRequests: 1,
                ErrorRequests: errorRequest ? 1 : 0,
                SuccessRequests: errorRequest ? 0 : 1,
                TotalTime: responseTime
            );

            _statistics.AddOrUpdate(request.Uri,
                addValueFactory: _ => stat,
                updateValueFactory: (_, oldValue) => UpdateStats(oldValue, stat)
            );
        }

        private static RequestStatistics UpdateStats(RequestStatistics oldValue, RequestStatistics increment)
        {
            return new RequestStatistics(
                oldValue.TotalRequests + increment.TotalRequests,
                oldValue.ErrorRequests + increment.ErrorRequests,
                oldValue.SuccessRequests + increment.SuccessRequests,
                oldValue.TotalTime + increment.TotalTime
            );
        }

        private static async Task WriteStatsToFileAsync()
        {
            // Asynchronous delay to reduce the likelihood of
            // failing to acquire the file due to parallel threads
            int delay = _rand.Next(5, 11) * 1000;
            await Task.Delay(delay);

            Dictionary<string, RequestStatistics> fileStats;

            try
            {
                using (var fs = new FileStream(StatisticsFilePath, FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None))
                {
                    try
                    {
                        fileStats = await JsonSerializer.DeserializeAsync
                            <Dictionary<string, RequestStatistics>>(fs) ?? new();
                    }
                    catch
                    {
                        fileStats = new Dictionary<string, RequestStatistics>();
                    }

                    foreach (var entry in _statistics)
                    {
                        if (fileStats.TryGetValue(entry.Key, out var existingStats))
                        {
                            fileStats[entry.Key] = UpdateStats(existingStats, entry.Value);
                        }
                        else
                        {
                            fileStats[entry.Key] = entry.Value;
                        }
                    }

                    _statistics.Clear();

                    fs.SetLength(0);

                    await JsonSerializer.SerializeAsync(fs, fileStats,
                        new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch { }
        }
    }
}

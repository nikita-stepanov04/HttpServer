using Microsoft.Extensions.Logging;

namespace HttpServerCore
{
    public static class LoggerExtensions
    {
        public static IDisposable? BeginRequestScope(this ILogger logger, Guid requestId)
        {
            return logger.BeginScope(new Dictionary<string, string>
            {
                { "RequestId", requestId.ToString() }
            });
        }

        public static IDisposable? BeginConnectionScope(this ILogger logger, Guid connectionId)
        {
            return logger.BeginScope(new Dictionary<string, string>
            {
                { "ConnectionId", connectionId.ToString() }
            });
        }
        
        public static IDisposable? BeginServerScope(this ILogger logger, int port)
        {
            return logger.BeginScope(new Dictionary<string, string>
            {
                { "ServerPort", port.ToString() }
            });
        }
    }
}

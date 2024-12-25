using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
namespace Configuration
{
    public static class CommonSettingsConfiguration
    {
        public static ILoggerFactory SerilogLoggerFactory = new SerilogLoggerFactory(
            new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger()
        );
    }
}

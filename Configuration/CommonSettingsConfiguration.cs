using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;
namespace Configuration
{
    public static class CommonSettingsConfiguration
    {
        public static string ConnectionString { get; } = "Host=localhost;Database=HttpServer;Username=postgres;Password=Password123$";

        public static ILogger Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.PostgreSQL(
                connectionString: ConnectionString,
                tableName: "ServerLogs",
                columnOptions: ColumnWriters,
                needAutoCreateTable: true)
            .CreateLogger();

        public static IDictionary<string, ColumnWriterBase> ColumnWriters { get; } = new Dictionary<string, ColumnWriterBase>
        {
            {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            {"timestamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
            {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            {"source_context", new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.ToString, NpgsqlDbType.Text) },
            {"scope", new SinglePropertyColumnWriter("Scope", PropertyWriteMethod.Json, NpgsqlDbType.Jsonb) }
        };
    }
}

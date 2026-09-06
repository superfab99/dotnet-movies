using Serilog;

namespace Movies.Api.Utils
{
    public static class Serilogs
    {
        public static void ConfigureSerilog(this IHostBuilder host)
        {
            host.UseSerilog((context, logConfig) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                logConfig.WriteTo.Console();
                // logConfig.WriteTo.MSSqlServer(connectionString,
                // sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                // {
                //     TableName = "Logs",
                //     AutoCreateSqlTable = true
                // });
            });
        }
    }
}
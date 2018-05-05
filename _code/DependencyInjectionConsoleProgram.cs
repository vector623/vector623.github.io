static class Program
{
    public static IConfigurationRoot Configuration { get; }
    public static ILoggerFactory LoggerFactory { get; }
    public static DbConnectionStringBuilder DbConnectionStringBuilder { get; }
    static Program()
    {
        Configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables("TROICENETDEV_")
            .Build();         
        DbConnectionStringBuilder = new NpgsqlConnectionStringBuilder()
        {
            Host = Configuration["PG_HOST"],
            Port = Convert.ToInt32(Configuration["PG_PORT"]),
            Database = Configuration["PG_DATABASE"],
            Username = Configuration["PG_USER"],
            Password = Configuration["PG_PASSWORD"],
        };        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        LoggerFactory = new LoggerFactory()
            .AddSerilog();        
    }
    
    static void Main(string[] args)
    {
        using (var statusController = new StatusController(DbConnectionStringBuilder, LoggerFactory))
        {
            var status = statusController.DataSyncStatus();
        }
    }
}
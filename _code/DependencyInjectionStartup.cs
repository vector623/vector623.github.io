public class Startup
{
    public IConfigurationRoot Configuration { get; }
    public ILoggerFactory LoggerFactory { get; }
    public DbConnectionStringBuilder DbConnectionStringBuilder { get; }
    
    public Startup()
    {
        Configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables("TROICENET_")
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
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.PostgreSQL(DbConnectionStringBuilder.ConnectionString,"Serilog")
            .CreateLogger();
        LoggerFactory = new LoggerFactory()
            .AddSerilog();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddOptions()
            .AddSingleton<IConfiguration>(Configuration)
            .AddSingleton(DbConnectionStringBuilder)
            .AddSingleton(LoggerFactory)
            .AddMvc();
    }

    public void Configure(IApplicationBuilder application, IHostingEnvironment environment)
    {
        application
            .UseDefaultFiles()
            .UseStaticFiles()
            .Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
    }
}
public class StatusController : Controller
{
    public ILoggerFactory LoggerFactory { get; }
    public DbConnectionStringBuilder ConnectionStringBuilder { get; }
    public StatusController(DbConnectionStringBuilder connectionStringBuilder, ILoggerFactory loggerFactory)
    {
        ConnectionStringBuilder = connectionStringBuilder;
        LoggerFactory = loggerFactory;
    }
}
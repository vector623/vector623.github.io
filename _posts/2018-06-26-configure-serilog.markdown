---
layout: post
title:  ".NET Logging Part 1, Configuring Serilog"
date:   2018-06-26 10:10:00 -0500
categories: csharp
---

I have recently invested some time into adopting Serilog as my goto logging framework for .NET projects.  It seems 
mature, lightweight and flexible enough to accommodate most projects, based on my experience.

## Log levels

Serilog specifies 7 layers of logging, meaning that anything that is logged must fall into one of the following 
enumerations of `LogEventLevel`.  This is Serilog's implementation (which closely mirror's Microsoft's [.NET Core
implementation][0])

```csharp
namespace Serilog.Events
{
  /// <summary>
  /// Specifies the meaning and relative importance of a log event.
  /// </summary>
  public enum LogEventLevel
  {
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Fatal,
  }
}
```

You'll pick a level when logging a message and also when deciding the minimum level that should be sent to a sink 
(more on sinks later).  For example, if you are just starting a project, you will probably want to log messages to
a local file and the way you'll do that is by tagging those messages with Debug and/or Verbose.  But when recording 
messages in a production context, you'll probably want to focus on Errors and exclude Debug messaging.

## Example configuration

Here is an example of Serilog configuration, which is run at the beginning of a program:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.MSSqlServer(
        loggingConnectionStringBuilder.ConnectionString, 
        "Ibex", 
        LogEventLevel.Information)
    .CreateLogger();
```

In this example, I am specifying that `Debug` is my minimum logging level, meaning that only `Verbose` messages will be
excluded.  Everything else will be recorded.  However, I can raise the minimum logging level depending on where the
log is going.  Here, I am recording debug-level logs to my program console, but the logs going to the database (MS Sql 
Server) must be Information level or higher.

After you've done this, you can now begin logging using the `Log` global object in this way:

```csharp
try
{
    Log.Information("test");
}
catch (Exception ex)
{
    Log.Error(ex, "exception thrown");
}
```

This example won't actually throw an exception, but this is the bare minimum for catching and recording exceptions via
logging.

### Configuring using Dependency Injection

Given that logging preferences will change between development/debugging, testing and production, having a single call 
to setup a single global logging object isn't ideal.  It'd be better to adjust the behavior of your logging in order to 
accommodate the context your project is running in and you can do this using Dependency Injection.  

Microsoft has produced useful articles [here][1] and [here][2] and they are both worth a read.  I've also written a 
[post][3] demonstrating how to setup DI in an .Net Core solution, using project to house business logic for a production 
context and also using a console project to debug..

## Multiple outputs

A major component of Serilog's flexiblity owes to the separation between what you are logging from where you are logging
to.  Suppose that we wanted to send our logs to Azure, which provides a useful query interface over your logged data.  
We can accomplish this by updating the above call in the following way:

```csharp
/*
 * This will cause all of our logged messages to be outputted to the console 
 * window, Sql Server and Azure.
 */
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.MSSqlServer(
        loggingConnectionStringBuilder.ConnectionString, 
        "Ibex", 
        LogEventLevel.Information)
    .WriteTo.AzureAnalytics(
        workspaceId: Configuration["AZUREANALYTICS_WORKSPACEID"],
        authenticationId: Configuration["AZUREANALYTICS_AUTHENTICATIONID"],
        logName: "ibex",
        restrictedToMinimumLevel: LogEventLevel.Debug, 
        batchSize: 10)
    .CreateLogger();
```


You'll need to install the `Serilog.Sinks.AzureAnalytics` package and configure a logging service in Azure, but once 
this is done, the new sink can be outputted to with a single call added to the Serilog initializer.  You can search
Nuget for `Serilog.Sinks` for a list of available logging outputs.  

Here's another example, which enables output to email:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.MSSqlServer(
        loggingConnectionStringBuilder.ConnectionString, 
        "Ibex", 
        LogEventLevel.Information)
    .WriteTo.Email( 
        //notice that we should only email the most urgent messages
        restrictedToMinimumLevel: LogEventLevel.Fatal, 
        connectionInfo: new EmailConnectionInfo()
        {
            MailServer = "mercury.nbsuply.com",
            FromEmail = "serilog@hmwallace.com",
            ToEmail = "",
            EmailSubject = "Serilog subject",
        })
    .CreateLogger();
```

## Formatting log messages

The more complicated logging sinks (SQL Server, PostGreSQL, Azure Log Analytics) allow you to query your logs, but you
will need to format those messages as you log them.

```csharp
//example of unformatted logging
Log.Logger.Information("unformatted message. logged on" + DateTime.Now.ToString("yyyy-MM-dd"));

//example of formatted logging
Log.Logger.Information("formatted message. {@data}", new { loggedOn = DateTime.Now });
```

If you log strings that you have manually crafted (first example), they will appear as strings in Azure Log Analytics, 
SQL Server or wherever the logs are saved.  You'll be limited to string filters when querying.  However, if you format 
your log messages, you will be able to query `loggedOn` as a timestamp in certain sinks.

In the case of SQL Server, formatted output is saved as an XML column, which we can then query in this way:

```sql
SELECT
  "Id",
  "TimeStamp",
  "Level",
  "Message",
  "MessageTemplate",
  Properties.value('(/properties/property[@key="data"]/structure/property[@key="loggedOn"])[1]', 'datetime') loggedOn
FROM ErrorLogging.dbo.Ibex
WHERE id IN (4352, 4353)
ORDER BY TimeStamp DESC
```

The resulting output looks something like this:

Id|TimeStamp|Level|Message|MessageTemplate|loggedOn
--- | --- | --- | --- | --- | ---   
4353|2018-06-26 11:05:06.0832878 -04:00|Information|formatted message. { loggedOn: 06/26/2018 11:05:05 }|formatted message. {@data}|2018-06-26 11:05:05.000
4352|2018-06-26 11:05:05.8617763 -04:00|Information|unformatted message. logged on2018-06-26|unformatted message. logged on2018-06-26

And here is the properties column for the formatted row:

```xml
<properties>
    <property key="data">
        <structure type="">
            <property key="loggedOn">6/26/2018 11:05:05 AM</property>
        </structure>
    </property>
</properties>
```

I've used SQL Server as an example for brevity, but in general I don't prefer parsing XML in SQL.  However, the
PostGreSQL Serilog Sink will store the equivalent data as JSON, which is far more readable and more easily queried.
Additionally, Azure Log Analytics has a separate query interface for logged data.  There are other cloud-based
monitoring platforms, like New Relic, which offer similar functionality (and also have Serilog Sinks in Nuget).

In the next post, I will demonstrate logging in a normal business ETL, using `try/catch/finally` and `ExpandoObject`.

[0]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=aspnetcore-2.1
[1]: https://msdn.microsoft.com/en-us/magazine/mt707534.aspx
[2]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1
[3]: https://vector623.github.io/csharp/2018/05/05/dependency-injection-example.html
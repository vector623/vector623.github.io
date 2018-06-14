---
layout: post
title:  ".NET Logging, Part 1: Configuring Serilog"
date:   2018-06-14 15:30:00 -0500
categories: csharp
---

.NET provides basic structures to handle lot of common development challenges.  One of those


## Log levels

.NET specifies 7 layers of logging, meaning that anything that is logged must fall into one of the following 
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
        .Destructure.ByTransforming<ExpandoObject>(e => new Dictionary<string,object>(e))
        .WriteTo.MSSqlServer(loggingConnectionStringBuilder.ConnectionString, "Ibex", LogEventLevel.Information)
        .WriteTo.Console(LogEventLevel.Debug)
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

## Multiple sinks

[0]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=aspnetcore-2.1
[1]: https://msdn.microsoft.com/en-us/magazine/mt707534.aspx
[2]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1
[3]: https://vector623.github.io/csharp/2018/05/05/dependency-injection-example.html
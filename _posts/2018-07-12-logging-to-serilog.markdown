---
layout: post
title:  ".NET Logging Part 2, Using Serilog"
date:   2018-07-12 10:10:00 -0500
categories: csharp
---

In context of backend ETLs, I try to keep my code readable and reduce the complexity and number of `try/catch/finally`
statements.  There are criticism against this, but my preference is to wrap an entire method in one large
`try/catch/finally` block.  I've found that combining functional development practices and Serilog enables robust
logging with minimal code clutter (and effort).

Here's an example of a typical ETL built into an Asp.Net MVC application, which pulls transactions from a source
database and inserts them to a remote one:

<script src="https://gist.github.com/vector623/6b9d960fb7aa2b5f7d696bfc4709da98.js"></script>

The first thing you should notice is the `dynamic ExpandoObject` which I am using to aggregate logged information. This
dynamic type allows you to assign values to member variables without having to first instantiate them.  I don't
generally advocate the use of `dynamic` objects in programming in general, since you lose a lot of assurances that
automatically come with static typing (easy and definitive refactoring goes right out the window, for example).  But in
this case, it helps us avoid having to define a separate class to contain the execution results of this one method.

`dynamic ExpandoObject` probably isn't the only way to succinctly aggregate logging info.  You could probably achieve
the same results with `Dictionary<string,object>`.

At the end of this process, I then convert the `dynamic ExpandoObject` to a `static` one with a simple cast.  Since this
process is being called in an ASP.NET application, I then convert the `ExpandoObject` to JSON and return all the logged
text in that form to the web client initiating the process.

If there is an exception, that is logged and in the `finally` block, all the aggregated data is logged.

Whether or not the ETL succeeds or fails at any point in the process, you will still have an aggregate of logged data
which can be referenced later on.  In the case of success, a summary of all work that has been done will be recorded.
In the case of failure, you will have a both the exception that was thrown and also a partial aggregate of the work that
was done before the process failed.  If it isn't clear from the exception where this process failed, you can use the
partially logged data to help figure out when things went wrong.

For example, if the logged data only has values for `newTransactionIdsCount` and `updatedTransactionIdsCount`, then the
ETL probably failed when it tried to fetch the transactions by their ids, on line 43.

Additionally, because Serilog allows for smart formatting of logged data and depending on the sink, you will be able to
query by any of the logged variables in the `dynamic ExpandoObject`.  Formatting output comes very handy when logging to
SQL Server, PostGreSQL, Azure Application Insights/Log Analytics, AWS Cloudwatch, New Relic, etc.

Here is a screencap of formatted output that has been logged to Azure Log Analytics:

![azure log analytics screencap](/images/azureLogs.png "Azure Log Analytics Screencap")

I've blurred out sensitive data, but this is a production log that was generated using the above technique. The
`selectQuery`, `s3bucket` (this is data being uploaded to Redshift, via Amazon's S3), `tableName`, `primaryKey` of the
table and the result of the ETL (in this case, a failure) have all been recorded.

Cut off in the screenshot, but included in the same log, is a property labeled `LogMessage_s` with the following
contents (this was pulled from a different log than the one in the screenshot above, but the format is the same):

```json
"{
  \"selectQuery\": \"SELECT * FROM dbo.SourceTable WHERE convert(DATE, pulled_at) IN ('2017-11-23','2017-11-24','2017-11-25','2017-11-27','2017-12-04') ORDER BY Id\",
  \"s3bucket\": \"s3Bucket/20180712090743.057\",
  \"tableName\": \"dbo.SourceTable\",
  \"primaryKey\": \"id\",
  \"result\": \"failure\"
}"
```

In order to structure the `LogMessage` property in this way, you need to add a `Destructure` line to Serilog's
configuration:

<script src="https://gist.github.com/vector623/916011088f619e41c8461e8436113561.js"></script>
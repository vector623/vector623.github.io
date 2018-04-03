---
layout: post
title:  "Format asp.net controller output for nightly jobs, part 2"
date:   2018-04-02 13:45:00 -0500
categories: csharp
---

Following my previous post, this code sample adds more detail to a project's execution details, for overnight jobs run
on a schedule:
```csharp
public ActionResult RunNightly()
{
    using (var shoppingController = new CampaignController())
    using (var reportsController = new ReportsController())
    {
        var actionsToRun = new Func<ActionResult>[]
        {
            reportsController.SyncShoppingReportsAllTestBrands,
            reportsController.SyncShoppingConversions,
        };
        var actionResults = actionsToRun
            .Select(action =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var actionResult = action();
                stopWatch.Stop();
                
                var executionDetails = new
                {
                    job = action.Method.Name,
                    result = ((ContentResult) actionResult).Content,
                    execTime = $"{stopWatch.Elapsed.Seconds}.{stopWatch.Elapsed.Milliseconds:000} seconds",
                };

                return executionDetails;
            })
            .ToList();
        var resultsJson = JToken
            .Parse(JsonConvert.SerializeObject(actionResults))
            .ToString();
        return Content(resultsJson, "application/json");
    }
}
```

The methods I want to run, sequentially, are listed in order and stored in an array, `actionsToRun`.  Linq is then used
to iterate over this collection, running each one and recording the execution details, which are then pretty-printed and
returned as json content.

The output for a json response like this will be something like the following: 
```json
[
  {
    "job": "SyncShoppingReportsAllTestBrands",
    "result": "Success. 0 rows updated",
    "execTime": "2.956 seconds"
  },
  {
    "job": "SyncShoppingConversions",
    "result": "Success. 6734 rows updated",
    "execTime": "51.380 seconds"
  }
]
```

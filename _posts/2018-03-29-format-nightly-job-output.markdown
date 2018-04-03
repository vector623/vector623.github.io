---
layout: post
title:  "Format asp.net controller output for nightly jobs"
date:   2018-03-29 11:30:00 -0500
categories: csharp
---

I prefer to call jobs from mvc asp.net controller actions and I also prefer to call those actions from another 
controller dedicated to grouping jobs in scheduled runs.

Here is how to return formatted json from an asp.net mvc project:
```csharp
public ActionResult RunNightly()
{
    using (var shoppingController = new CampaignController())
    using (var reportsController = new ReportsController())
    {

        var resultsJson = JsonConvert
            .SerializeObject(new[]
            {
                new
                {
                    job = "syncShoppingReportsAllTestBrands",
                    result = 
                        (reportsController.SyncShoppingReportsAllTestBrands() as ContentResult).Content
                },
                new
                {
                    job = "updateProductPartitionsCube",
                    result = 
                        (reportsController.UpdateProductPartitionsCube() as ContentResult).Content
                },
            });

        return Content(JToken.Parse(resultsJson).ToString(), "application/json");
    }
}
```

The output for a json response like this will be something like the following: 
```json
[
  {
    "job": "syncShoppingReportsAllTestBrands",
    "result": "Success. 0 rows updated"
  },
  {
    "job": "updateProductPartitionsCube",
    "result": "Failure: exception message: Invalid column name 'allConversions'.\r\nInvalid column name 'allConversionsValue'.; inner exception message = null"
  }
]
```

So when I come to work in the morning and notice a job has failed, I can simply visit the url that ran the nightly jobs
to see which specific job caused the problem, along w/a useful error message.
---
layout: post
title:  ".NET Logging Part 2, Serilog Usage Example"
date:   2018-06-28 09:00:00 -0500
categories: csharp
---

You can combine `try/catch/finally`, `dynamic` types, `ExpandoObjects` and json output to really nail down logging.

Here is an example of a typical ETL written in Asp.NET (with some details removed for brevity):

```csharp
public ContentResult ImportAmazonOrders()
{
    var listOrdersRequest = amazonClient
        .GetListOrdersRequest()
        .WithMaxResultsPerPage(100)
        .WithCreatedAfter(DateTime.Today.AddDays(-7))
        .WithOrderStatus(new string[] { "Unshipped", "PartiallyShipped", "Shipped" });
    var currentAmazonOrders = amazonClient
        .DownloadOrdersFromAmazonAggUntil(listOrdersRequest)
        .ToList();
    var downloadedOrderIds = dbClient
        .GetAmazonOrderIds()
        .ToList();

    var asinItemIdMaps = dbClient
        .GetAsinItemIdMaps()
        .OrderBy(x => x, new AsinItemIdMap.IdentityComparer())
        .ToList();
    var newAmazonOrders = currentAmazonOrders
        .Where(amazonOrder => !downloadedOrderIds.Contains(amazonOrder.AmazonOrderId))
        .OrderByDescending(ao => ao.PurchaseDate)
        .Take(25)
        .ToList();
    var newAmazonOrdersItems = newAmazonOrders
        .Select(newAmazonOrder =>
        {
            var listOrderItemsRequest = amazonClient
                .GetListOrderItemsRequest()
                .WithAmazonOrderId(newAmazonOrder.AmazonOrderId);
            var orderItems = amazonClient
                .DownloadOrderItemsFromAmazonAggUntil(listOrderItemsRequest)
                .ToList();

            var items = new AmazonOrderWithItems
            {
                order = newAmazonOrder.ConvertToAmazonMwsSalesOrder(),
                orderItems = orderItems
                    .Select(orderItem => orderItem.ConvertToAmazonMwsSalesOrderItem(
                        newAmazonOrder.AmazonOrderId, asinItemIdMaps))
                    .ToList(),
            };
            return items;
        })
        .ToList();
    var newAmazonOrdersWithItems = newAmazonOrdersItems
        .Where(newAmazonOrderItems => newAmazonOrderItems.orderItems.Count > 0)
        .ToList();
    var insertResult = dbClient.InsertAmazonOrdersItems(newAmazonOrdersWithItems);

    return Content("success");
}
```

This ETL is basically fetching a list of orders from Amazon, then running a diff, to find the new orders, then for the
top 25 new orders, it is fetching the line items for those orders, then inserting them into the database.

As you can see, there are several steps to this process and it's possible for any of these to fail.  The first
improvement will be to handle exceptions:

```csharp
public ContentResult ImportAmazonOrders()
{
    try
    {
        var listOrdersRequest = amazonClient
            .GetListOrdersRequest()
            .WithMaxResultsPerPage(100)
            .WithCreatedAfter(DateTime.Today.AddDays(-7))
            .WithOrderStatus(new string[] { "Unshipped", "PartiallyShipped", "Shipped" });
        var currentAmazonOrders = amazonClient
            .DownloadOrdersFromAmazonAggUntil(listOrdersRequest)
            .ToList();
        var downloadedOrderIds = dbClient
            .GetAmazonOrderIds()
            .ToList();

        var asinItemIdMaps = dbClient
            .GetAsinItemIdMaps()
            .OrderBy(x => x, new AsinItemIdMap.IdentityComparer())
            .ToList();
        var newAmazonOrders = currentAmazonOrders
            .Where(amazonOrder => !downloadedOrderIds.Contains(amazonOrder.AmazonOrderId))
            .OrderByDescending(ao => ao.PurchaseDate)
            .Take(25)
            .ToList();
        var newAmazonOrdersItems = newAmazonOrders
            .Select(newAmazonOrder =>
            {
                var listOrderItemsRequest = amazonClient
                    .GetListOrderItemsRequest()
                    .WithAmazonOrderId(newAmazonOrder.AmazonOrderId);
                var orderItems = amazonClient
                    .DownloadOrderItemsFromAmazonAggUntil(listOrderItemsRequest)
                    .ToList();

                var items = new AmazonOrderWithItems
                {
                    order = newAmazonOrder.ConvertToAmazonMwsSalesOrder(),
                    orderItems = orderItems
                        .Select(orderItem => orderItem.ConvertToAmazonMwsSalesOrderItem(
                            newAmazonOrder.AmazonOrderId, asinItemIdMaps))
                        .ToList(),
                };
                return items;
            })
            .ToList();
        var newAmazonOrdersWithItems = newAmazonOrdersItems
            .Where(newAmazonOrderItems => newAmazonOrderItems.orderItems.Count > 0)
            .ToList();
        var insertResult = dbClient.InsertAmazonOrdersItems(newAmazonOrdersWithItems);

        return Content("success");
    }
    catch (Exception ex)
    {
        return Content("failure");
    }
}
```

Simply wrapping the entire process in a large `try/catch` block is generally considered bad form, but I'd argue that
wrapping each individual step in its own `try/catch` is as problematic.  Fortunately, we're not done.  We should now
pack the results of each step into a `dynamic` object that is instantiated prior to the `try` block

```csharp
public ContentResult ImportAmazonOrders()
{
    var listOrdersRequest = amazonClient
        .GetListOrdersRequest()
        .WithMaxResultsPerPage(100)
        .WithCreatedAfter(DateTime.Today.AddDays(-7))
        .WithOrderStatus(new string[] { "Unshipped", "PartiallyShipped", "Shipped" });
    var currentAmazonOrders = amazonClient
        .DownloadOrdersFromAmazonAggUntil(listOrdersRequest)
        .ToList();
    var downloadedOrderIds = dbClient
        .GetAmazonOrderIds()
        .ToList();

    var asinItemIdMaps = dbClient
        .GetAsinItemIdMaps()
        .OrderBy(x => x, new AsinItemIdMap.IdentityComparer())
        .ToList();
    var newAmazonOrders = currentAmazonOrders
        .Where(amazonOrder => !downloadedOrderIds.Contains(amazonOrder.AmazonOrderId))
        .OrderByDescending(ao => ao.PurchaseDate)
        .Take(25)
        .ToList();
    var newAmazonOrdersItems = newAmazonOrders
        .Select(newAmazonOrder =>
        {
            var listOrderItemsRequest = amazonClient
                .GetListOrderItemsRequest()
                .WithAmazonOrderId(newAmazonOrder.AmazonOrderId);
            var orderItems = amazonClient
                .DownloadOrderItemsFromAmazonAggUntil(listOrderItemsRequest)
                .ToList();

            var items = new AmazonOrderWithItems
            {
                order = newAmazonOrder.ConvertToAmazonMwsSalesOrder(),
                orderItems = orderItems
                    .Select(orderItem => orderItem.ConvertToAmazonMwsSalesOrderItem(
                        newAmazonOrder.AmazonOrderId, asinItemIdMaps))
                    .ToList(),
            };
            return items;
        })
        .ToList();
    var newAmazonOrdersWithItems = newAmazonOrdersItems
        .Where(newAmazonOrderItems => newAmazonOrderItems.orderItems.Count > 0)
        .ToList();
    var insertResult = dbClient.InsertAmazonOrdersItems(newAmazonOrdersWithItems);

    return Content("success");
}
```

This ETL is basically fetching a list of orders from Amazon, then running a diff, to find the new orders, then for the
top 25 new orders, it is fetching the line items for those orders, then inserting them into the database.

As you can see, there are several steps to this process and it's possible for any of these to fail.  The first
improvement will be to handle exceptions:

```csharp
public ContentResult ImportAmazonOrders()
{
    dynamic logMe = new ExpandoObject();
    try
    {
        logMe.listOrdersRequest = amazonClient
            .GetListOrdersRequest()
            .WithMaxResultsPerPage(100)
            .WithCreatedAfter(DateTime.Today.AddDays(-7))
            .WithOrderStatus(new string[] { "Unshipped", "PartiallyShipped", "Shipped" });
        logMe.currentAmazonOrders = amazonClient
            .DownloadOrdersFromAmazonAggUntil(listOrdersRequest)
            .ToList();
        logMe.downloadedOrderIds = dbClient
            .GetAmazonOrderIds()
            .ToList();

        logMe.asinItemIdMaps = dbClient
            .GetAsinItemIdMaps()
            .OrderBy(x => x, new AsinItemIdMap.IdentityComparer())
            .ToList();
        logMe.newAmazonOrders = currentAmazonOrders
            .Where(amazonOrder => !downloadedOrderIds.Contains(amazonOrder.AmazonOrderId))
            .OrderByDescending(ao => ao.PurchaseDate)
            .Take(25)
            .ToList();
        logMe.newAmazonOrdersItems = newAmazonOrders
            .Select(newAmazonOrder =>
            {
                var listOrderItemsRequest = amazonClient
                    .GetListOrderItemsRequest()
                    .WithAmazonOrderId(newAmazonOrder.AmazonOrderId);
                var orderItems = amazonClient
                    .DownloadOrderItemsFromAmazonAggUntil(listOrderItemsRequest)
                    .ToList();

                var items = new AmazonOrderWithItems
                {
                    order = newAmazonOrder.ConvertToAmazonMwsSalesOrder(),
                    orderItems = orderItems
                        .Select(orderItem => orderItem.ConvertToAmazonMwsSalesOrderItem(
                            newAmazonOrder.AmazonOrderId, asinItemIdMaps))
                        .ToList(),
                };
                return items;
            })
            .ToList();
        logMe.newAmazonOrdersWithItems = newAmazonOrdersItems
            .Where(newAmazonOrderItems => newAmazonOrderItems.orderItems.Count > 0)
            .ToList();
        logMe.insertResult = dbClient.InsertAmazonOrdersItems(newAmazonOrdersWithItems);

        logMe.result = "success";
        return Content(
            JsonConvert.SerializeObject((ExpandoObject)logMe,Formatting.Indented),
            "application/json"
        };
    }
    catch (Exception ex)
    {
        logMe.result = "failure";
        return Content(
            JsonConvert.SerializeObject((ExpandoObject)logMe,Formatting.Indented),
            "application/json"
        };
    }
}
```

See what I did there?  I've stashed the results of every function result into the `logMe` dynamic variable, which, 
because it's dynamic, does not require member variables to be declared before they're set.  Then, I am converting the
`dynamic ExpandoObject` and returning it as a JSON string.

The benefits to doing this is that the resulting JSON will contain a comprehensive report of everything that was done
during this process.  And if it fails at any point, the work that was done will still be reported, based on how I've
setup the `logMe` object.  If any exceptions are encountered, you will be able to determine where those errors resulted,
based on which report variables haven't been populated in the `logMe` object.

Finally, you can configure Serilog to capture all of the logged variables:

```csharp
public ContentResult ImportAmazonOrders()
{
    dynamic logMe = new ExpandoObject();
    try
    {
        logMe.listOrdersRequest = amazonClient
            .GetListOrdersRequest()
            .WithMaxResultsPerPage(100)
            .WithCreatedAfter(DateTime.Today.AddDays(-7))
            .WithOrderStatus(new string[] { "Unshipped", "PartiallyShipped", "Shipped" });
        logMe.currentAmazonOrders = amazonClient
            .DownloadOrdersFromAmazonAggUntil(listOrdersRequest)
            .ToList();
        logMe.downloadedOrderIds = dbClient
            .GetAmazonOrderIds()
            .ToList();

        logMe.asinItemIdMaps = dbClient
            .GetAsinItemIdMaps()
            .OrderBy(x => x, new AsinItemIdMap.IdentityComparer())
            .ToList();
        logMe.newAmazonOrders = currentAmazonOrders
            .Where(amazonOrder => !downloadedOrderIds.Contains(amazonOrder.AmazonOrderId))
            .OrderByDescending(ao => ao.PurchaseDate)
            .Take(25)
            .ToList();
        logMe.newAmazonOrdersItems = newAmazonOrders
            .Select(newAmazonOrder =>
            {
                var listOrderItemsRequest = amazonClient
                    .GetListOrderItemsRequest()
                    .WithAmazonOrderId(newAmazonOrder.AmazonOrderId);
                var orderItems = amazonClient
                    .DownloadOrderItemsFromAmazonAggUntil(listOrderItemsRequest)
                    .ToList();

                var items = new AmazonOrderWithItems
                {
                    order = newAmazonOrder.ConvertToAmazonMwsSalesOrder(),
                    orderItems = orderItems
                        .Select(orderItem => orderItem.ConvertToAmazonMwsSalesOrderItem(
                            newAmazonOrder.AmazonOrderId, asinItemIdMaps))
                        .ToList(),
                };
                return items;
            })
            .ToList();
        logMe.newAmazonOrdersWithItems = newAmazonOrdersItems
            .Where(newAmazonOrderItems => newAmazonOrderItems.orderItems.Count > 0)
            .ToList();
        logMe.insertResult = dbClient.InsertAmazonOrdersItems(newAmazonOrdersWithItems);

        logMe.result = "success";
        return Content(
            JsonConvert.SerializeObject((ExpandoObject)logMe,Formatting.Indented),
            "application/json"
        };
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Error downloading amazon orders");
        logMe.result = "failure";
        return Content(
            JsonConvert.SerializeObject((ExpandoObject)logMe,Formatting.Indented),
            "application/json"
        };
    }
    finally 
    {
        Log.Logger.Information("{@resultsProperties}",logMe);
    }
}
```
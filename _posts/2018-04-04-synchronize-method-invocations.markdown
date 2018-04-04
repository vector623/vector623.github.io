---
layout: post
title:  "Synchronize method invocations"
date:   2018-04-04 11:45:00 -0500
categories: csharp
---

When I want to synchronized method invocations, I often want to cancel an attempt if the method is already running, as
opposed to queueing up another execution instance.  Here's an example of how to do that:

```csharp
public ActionResult Test()
{
    var semaphore = new Semaphore(1, 1, MethodBase.GetCurrentMethod().Name);
    if (!semaphore.WaitOne(0))
    {
        return Content($"failure: method is already running in another context");
    }
    
    try
    {
        Thread.Sleep(5 * 1000);
        return Content("success");
    }
    catch (Exception e)
    {
        return Content($"failure: {e.Message}");
    }
    finally
    {
        semaphore.Release();
    }
}
```

This method will run and take 5 seconds to complete.  If this method is run again during that time, it will return 
without executing the `try/catch/finally` block, with an error message. Also, `finally` block is required to release the 
semaphore so that this method can be executed again.

When testing concurrency of a web app, keep in mind that many browsers won't run multiple page fetches to
the same site simultaneously.


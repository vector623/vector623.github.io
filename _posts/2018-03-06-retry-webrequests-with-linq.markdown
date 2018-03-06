---
layout: post
title:  "Retry webrequests with linq"
date:   2018-03-06 08:29:00 -0500
categories: jekyll update
---
I prefer functional programming whenever possible.  Inevitably, there are exceptions that have to be made to the 
functional paradigm and I thought that calling web APIs was one of those exceptions.

As it turns out, functional programming (specifically, using linq with C#) is able to handle re-attempting requests 
against web APIs quite well:
<script src="https://gist.github.com/vector623/7f6d903ea73df4d227ec587412f83a4f.js"></script>

In this example, I am using `Enumerable.Range()` to create a set of numbers, 0 through 4.  I then use Linq's `Select()` 
method to iterate through those integers, storing those integers in the variable `pageAttempt`, for each iteration.

I've setup a try/catch block in order to contend with rate limit requests. If you are working with a service API, it's 
inevitable that API calls will throw rate limit exceptions. Sometimes the exception will contain a timeout value, but in 
my experience, it usually won't.

A good practice then, when interacting with rate-limited APIs, is to to exponentially increase your wait times between 
each API request.  That's how the `pageAttempt` variable is used.  On the first iteration, `Thread.Sleep()` will suspend 
execution for 0 milliseconds, then 2.5 seconds, then 10 seconds and so on.

Finally, linq's `First()` method will halt iteration when a valid result has been fetched.  In this context, `First()` 
operates similarly to a break statement within a while loop.  Additionally, if all iterations are run and only null
results are returned from `Select()`, the `First()` method will throw an exception, which should be caught in the 
surrounding context and treated as if the API request has failed entirely.



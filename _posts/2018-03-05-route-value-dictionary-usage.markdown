---
layout: post
title:  "Quickly deanonymize dynamic typing"
date:   2018-03-05 16:52:00 -0500
categories: csharp
---
Dynamic types are useful when extracting values from data sources before structuring them, but I generally want to 
return to a static-typed context as soon as possible.  .NET provides a built-in type, [RouteValueDictionary][1], which
lets you extract key-value pairs from a dynamic type object pretty easily.

Example code:
<script src="https://gist.github.com/vector623/327f84ec410a6b937fbbf23c08a9292c.js"></script>

In this gist, I am reading a Google AdWords report response object, returned from `reportRequest`, into a 
[`CsvReader`][2] object.  I am converting each record to a `dynamic` object, but quickly converting that object into
a `RouteValueDictionary` object in the next `Select()` statement.  

From there I am formatting and reading the individual
columns, creating a new class of `KeywordPerformanceStat`, which I will then insert into the database using [`Dapper`][3]

[1]: https://msdn.microsoft.com/en-us/library/system.web.routing.routevaluedictionary(v=vs.110).aspx
[2]: http://joshclose.github.io/CsvHelper/
[3]: https://github.com/StackExchange/Dapper
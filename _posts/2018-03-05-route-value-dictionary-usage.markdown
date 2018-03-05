---
layout: post
title:  "Quickly deanonymize dynamic typing"
date:   2018-03-05 16:52:00 -0500
categories: jekyll update
---
Dynamic types are useful when extracting values from data sources before structuring them, but I generally want to 
return to a static-typed context as soon as possible.  .NET provides a built-in type, [RouteValueDictionary][1], which
lets you extract key-value pairs from a dynamic type object pretty easily.

Example code:
<script src="https://gist.github.com/vector623/327f84ec410a6b937fbbf23c08a9292c.js"></script>

[1]: https://msdn.microsoft.com/en-us/library/system.web.routing.routevaluedictionary(v=vs.110).aspx

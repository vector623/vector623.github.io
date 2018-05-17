---
layout: post
title:  "Quickly deanonymize dynamic typing, part 2"
date:   2018-05-16 09:30:00 -0500
categories: csharp
---

C#'s native and original web language is XML over SOAP and for .NET developers and this has always worked quite well,
given C#'s design as a static typed language and also given the reliable tooling made available by Microsoft through
Visual Studio.

However the SOAP+XML standard has largely been supplanted by JSON over REST APIs by dynamically typed languages and
platforms, like Ruby on Rails and Node.js.  C# doesn't comprehend JSON as well and the result can be a lack of
intellisense/code-completion and an inability to look inside objects in a debugging context.

Thankfully, Microsoft has provided some useful helpers that re-establish code completion and debugging when
deserializing JSON.

You can combine Newtonsoft's Json deserialization, combined with `RouteValueCollection` in order to deserialize complex
JSON objects into a static context, which will re-enable intellisense and allow you to easily examince objects through
Visual Studio's debugger:

Here's a quick demo:
<script src="https://gist.github.com/vector623/2780a6126a998fb093e437068e2dc63f.js"></script>

Here, I am reading a Json array from Github using `HttpClient`.  Then I'm deserializing that array to a `dynamic` type.
At this point (line 34-35) you will not yet have code completion or be able to examine the data in debugging.  This is
enabled by create a `RouteValueDictionary` type, which will convert the `dynamic` object to a `JArray` or `JObject`,

In this example, the `RouteValueDictionary` isn't fully necessary, since the response content can be deserialized
straight into to a `JArray`.  You can also see that the Root object of the `RouteValueDictionary` still has to be
converted into `JArray` anyways.

But in cases when your data doesn't fit into a `JArray`, you can still fall back onto `RouteValueDictionary` for
deanonymization.  This can be used to deanonymize data pulled from other sources, such as SQL results queried by
[Dapper][0], or rows read in from [CsvHelper][1].

Here is a [link to the Git repo][2] for this demo

[0]: https://github.com/StackExchange/Dapper
[1]: https://github.com/JoshClose/CsvHelper
[2]: https://github.com/vector623/GithubPullerDemo
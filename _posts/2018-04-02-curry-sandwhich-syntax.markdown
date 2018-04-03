---
layout: post
title:  "Curry sandwich syntax"
date:   2018-04-02 13:55:00 -0500
categories: csharp
---

Currying is a prominent feature of functional programming and I've come to strongly prefer stacking curry-methods in my
development.  Each curried method gets a single line and I find this to improve code readability.  The methods are 
curried and stacked on top of each other, so I call it Curry Sandwich Syntax.
  
Here is an example of curry sandwich syntax; I am grouping a large collection, `reportLines`, into batches of 2500.  `reportLineSlices` 
will be assigned to a list of lists, each containing 2500 report lines.  The last grouping will contain less than 2500 
elements, unless the number of elements in the `reportLines` collection is a multiple of 2500:

```csharp
var reportLinesSlices = reportLines
    .Where(reportLine => reportLine.Date >= DateTime.Parse("2018-01-01"))
    .Select((query, i) => new { SearchQuery = query, Index = i })
    .GroupBy(queryIndex => queryIndex.Index / 2500)
    .Select(grouping =>
        grouping
            .Select(g => g.SearchQuery)
            .ToList())
    .ToList();
```

Testing and debugging are often easier with this syntax as well, since you can test removal of  where clauses by 
commenting out a single line of code.  Contrast the below with disabling an if statement nested within a for/while loop.

```csharp
var reportLinesSlices = reportLines
    //.Where(reportLine => reportLine.Date >= DateTime.Parse("2018-01-01"))
    .Select((query, i) => new { SearchQuery = query, Index = i })
    .GroupBy(queryIndex => queryIndex.Index / 25000)
    .Select(grouping =>
        grouping
            .Select(g => g.SearchQuery)
            .ToList())
    .ToList();
```

I will often use the `Take()` method to limit the number of items I process, when developing an ETL.  For example, if I
want to limit the size of the batch I am processing to 10 elements, I will insert the following.

```csharp
var reportLinesSlices = reportLines
    .Where(reportLine => reportLine.Date >= DateTime.Parse("2018-01-01"))
    .Take(10)
    .Select((query, i) => new { SearchQuery = query, Index = i })
    .GroupBy(queryIndex => queryIndex.Index / 25000)
    .Select(grouping =>
        grouping
            .Select(g => g.SearchQuery)
            .ToList())
    .ToList();
```

Graduating the above code to production is as simple as deleting the line with `Take()` method call.

You can also debug lambda statements if you wrap the code block in curly brackets and return your result.  To do this,
change the above into the following (to be able to step into the second select statement during debugging):
```csharp
var reportLinesSlices = reportLines
    .Where(reportLine => reportLine.Date >= DateTime.Parse("2018-01-01"))
    .Select((query, i) => new { SearchQuery = query, Index = i })
    .GroupBy(queryIndex => queryIndex.Index / 25000)
    .Select(grouping => 
    {
        var queryGrouping = grouping
            .Select(g => g.SearchQuery)
            .ToList()
        return queryGrouping;
    })
    .ToList();
```

A breakpoint can now be set on the `queryGrouping` assignment, or on the return statement.  Curly braces can likewise be
applied to `Where()` statements, or any `Func` passed into a Linq method.  For example:

```csharp
var reportLinesSlices = reportLines
    .Where(reportLine => 
    {
        var thisYear = reportLine.Date >= DateTime.Parse("2018-01-01");
        return thisYear;
    })
    .Select((query, i) => new { SearchQuery = query, Index = i })
    .GroupBy(queryIndex => queryIndex.Index / 25000)
    .Select(grouping =>
        grouping
            .Select(g => g.SearchQuery)
            .ToList())
    .ToList();
```

A quality IDE (either Visual Studio or Jetbrains' Rider) should autofold bracketed lambda statements neatly, allowing 
you to hide messy implementation details to concentrate on the broader code.

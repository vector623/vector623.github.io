---
layout: post
title:  "Reduce until"
date:   2018-03-07 15:00:00 -0500
categories: csharp
---
In working with web APIs that return pages of results, I've found that they generally have these things in common: 

1. You need to read the first page in order to know how many subsequent queries are required to fetch the full result set.  You won't know how many fetches must be made before you read your first result.
    * You may not know how many total pages must be fetched until you've fetched the last one
2. You need data from the original query and/or the previous result page in order to fetch the next page.
3. Results need be added to a growing set from each page fetched.
4. You need include a break-condition, evaluated after each page fetched, to know when to stop fetching more pages.

The higher order function Reduce() can accommodate most of these requirements, but by itself can't both run an initial page fetch combined with a condition evaluation to stop additional pages at the right time.  I've modified C#'s Aggregate() method (which is Microsoft's version of Reduce()) to satisfy all of the above conditions.

AggregateUntil():

<script src="https://gist.github.com/vector623/9cf726e029c40de93b6052479e684d1d.js"></script>

AggregateUntil()'s parameters are:

1. `source` - This the object on which the extension method is called.  This should be a list of integers (see below example for why).
2. `seed` - An initial seed value.  This can be any class, which will contain the accumulated result set of all page fetches.
    * This object must also accommodate any web token provided by the API for additional page requests.
3. `func` - This is a code block that the iterator will run, which takes the following two parameters:
    * An object containing the accumulated result set (plus any web tokens).  It needs to be of the same type as the `seed` parameter
    * An integer representing the number of iterations that have been run.  This is like the i in a `for(int i = 0; i <...` loop
4. `untilFunc` - This is a function with one parameter, the accumulated result set object.  This `Func()` needs to return true when we are done retrieving results and false if there are more pages to fetch.

Here is an example use of AggregateUntil(), within the context of Google AdWords' API:

<script src="https://gist.github.com/vector623/6c7372bcad22c2a1c5e29c9bba4e5937.js"></script>

TODO: add an example for Amazon's Marketplace Web Service
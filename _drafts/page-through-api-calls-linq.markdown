---
layout: post
title:  "Paging through api calls with linq"
date:   2018-02-14 09:36:00 -0500
categories: jekyll update
---

I've spent a lot of time coding against web APIs.  In that time I've noticed that, while the details may shift between service providers and the web APIs they offer, reading numerous records from them have some things in common.

1. You will read partial sets of results that are returned in pages and you won't know how many pages of results you need to fetch until after the first call is made.  You may not know how many pages of results must be fetched until you've read the last one.
2. You will want to loop through the api call, augmenting the request with each iteration, based on data from a prior call.
3. You will need to craft a test in order to break out of the loop when you are done pulling results.
4. You need to deal with rate limit exceptions.

Here is an example of the kind of code I, and I imagine most other developers, automatically tend to.
<script src="https://gist.github.com/vector623/4c1b491b1df50fbf2ecfa15e7c2895d9.js"></script>

AdWords takes a start index and page size (see `LIMIT` clause) returns a `totalNumEntries` value that you can use to figure out when to stop looping.

You can see the open-ended while loop as a result of how I don't know going into the initial query, how many subsequent pages need to be pulled, and how the if statement at the bottom of the loop breaks when I'm done.  The AdWords has its own SQL-like query langauge, and so each iteration I am updating the LIMIT clause to advance the results returned to the next page.  And the for loop within the while loop retries queries if a rate exception is encountered.

I've written a lot of code like this and the above example is much tighter than what I wrote earlier in my career.  But even with my experience handling api result pages, things still get hairy when pulling from Amazon Marketplace.

<script src="https://gist.github.com/vector623/0b6aa888783a8049e61d02a0d5d32998.js"></script>

Instead of providing `totalNumEntries` and accepting a `LIMIT` clause, Amazon Marketplace provides and accepts a `NextToken` value, which must be successfully passed into subsequent loops.  The pattern that worked well enough for AdWords doesn't scale as well with Amazon.

I've established a Map-Reduce pattern for myself that applies well to both.
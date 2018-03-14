---
layout: post
title:  "Import with CsvHelper"
date:   2018-03-14 12:14:00 -0500
categories: jekyll update c#
---

CsvHelper is my preferred dependency for handling csv files, in C#.

Example CsvHelper usage:
<script src="https://gist.github.com/vector623/63a3c4e5feb6e322b58cb5c2ac94d143.js"></script>

Use `dynamic` to instantiate a record object for each row, then use `RouteValueDictionary` to assist with accessing each row's columns.
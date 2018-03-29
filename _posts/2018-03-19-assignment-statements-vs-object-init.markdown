---
layout: post
title:  "Assignment statements vs object initializers"
date:   2018-03-19 10:28:00 -0500
categories: jekyll update csharp
---

Creating and assigning values to object instances is done through one of two syntaxes. The more traditional and 
clunkier syntax is the assignment statement. Assignment statements typically begin with an empty constructor, followed 
by multiple assignments, one for each variable to be set. Here is an example:
<script src="https://gist.github.com/vector623/16542699bae26f18c108d4f82a56ad19.js"></script>

The newer syntax and much improved syntax is the object initializer. In a single statement, multiple variables are set
at the same time the object is instantiated.  The assignments are also tabbed, which allow for easier reading.  Here is
an example:
<script src="https://gist.github.com/vector623/7245bb6575b7f977f580bfc8d0eeee26.js"></script>
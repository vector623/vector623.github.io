---
layout: post
title:  "Example of dependency injection in ASP.Net Core"
date:   2018-05-05 12:30:00 -0500
categories: csharp
---

If you're new to ASP.Net Core, the application configuration in Program.cs and Startup.cs might come off as a bit confusing.  I originally bypassed 
it all but have lately had a change of heart regarding dependency injection.

Here is an example of a Startup.cs file using Dependency Injection:
<script src="https://gist.github.com/vector623/abf7216687462b96619430b3a3ad636c.js"></script>

In the Startup() constructor, I am instantiating a configuration object, a sql string builder object and a logger object and assigning them to member variables in the Startup class.  Then, in ConfigureServices(), which is called by ASP.Net core during initialization, I'm registering those objects to an IServiceCollection object.

The services registered here will then be automatically fed into my controller constructors.  Here's an example:
<script src="https://gist.github.com/vector623/5e9c70017a36d5e6b428bf2c46b9602d.js"></script>

The StatusController constructor, which ASP.Net Core will call as it's needed, will be fed a sql connection string generator and also a logging object generator, which are specified in the web app's initializers.  You can also add other parameters to your controllers and ASP.Net Core will automatically map what you've registered in Startup.cs to those parameters.

## Why make the effort?

As a backend dev, I typically prefer to deploy processes to web applications and to run them with web calls.  However, as I develop these processes, I prefer to debug through my IDE, hitting F5 to do so.  It's tedious to have to then visit the url mapped to my process in a browser tab in order to run my code, so I will add a console project to my solution, add the web app as a dependency and setup Program.cs (in my console/debug project) in this way:

<script src="https://gist.github.com/vector623/b313c6f531d5d87987ebfd156fb12219.js"></script>

Most of the initialization has been copied and pasted, but there are a few important differences.  First, I have adjusted the prefix for the environment variables from which I am importing database credentials.  Everything executed through the console application will be read and written against different database creds than what runs in production.

The second difference is to the logging.  In Startup.cs (production), I'm logging to a database, while in Program.cs (debug), the line to log to the database has been removed, since I prefer not to generate database logs everytime I hit F5 (which is often).  Also note that the minimum log level for production (Startup.cs) is set to 'Information', while the console project's minimum log level is 'Debug'.  I'd also adjust database creds, logging and other details for a unit test project.

You can also use this technique to point debug at a sandbox api endpoint, swap out a PostGreSQL database for a SQLite instance, and so on.

Any respectable application will require some tediousness to separate the production context from the development and testing contexts.  Based on my experience, Dependency Injection and Inversion of Control, which I've demonstrated here, is the ideal way to do that.
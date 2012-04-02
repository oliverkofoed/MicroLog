MicroLog is a small, fast and efficient logging framework for .NET.

It's small size and extensibility makes it easy to integrate and customize for your project needs. 

Features
========
* Small, Fast & Efficient
* Runtime configuration change support
* Cross app domain log event collection (send events to parent app domain)
* Easy to integrate: just include a few source files (or go oldschool with MicroLog.dll)
* Rolling file logging
* Very extendable

![](https://github.com/oliverkofoed/MicroLog/raw/master/ReadMe.ConsoleScreenshot.png)


Usage
======
Usage is dead simple. All you have to do is get a Logger
instance from one of two static get methods and use
it to log messages:

	// Get a logger named with the current class name
	Logger logger = Logger.GetClassLogger();

Or: 

	// Get a logger with a custom name 
	Logger logger = Logger.Get("My Fancy Logger");

Once you have it, you can use methods
corresponding to each log level to log information:

	logger.Debug("I'm debugging");
	logger.Info("I'm informing you");
	logger.Warn("I'm warning you...");
	logger.Error("I'm faulty. I err.");
	logger.Fatal("I failed comletely!");

There are also overloads for logging exceptions:

	try {
		// ...
	} catch(Exception e) {
		logger.ErrorException("The Operation Failed", e);
	}

Configuration
=============
You have to configure MicroLog before log messages appear anywhere.

To configure MicroLog via config files, you have to add a
microlog config section:

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
		...
		<configSections>
			<section name="microlog" type="MicroLog.ConfigSectionHandler, MicroLog"/>
		</configSections>
		<microlog>
			<target type="console"		minlevel="debug" async="true" />
			<target type="file"			minlevel="info" file="Logs/main_$starttime.txt" />
			<target type="exceptions"	minlevel="info" file="Logs/exceptions_$starttime.txt" />
		</microlog>
		...
	</configuration>

And then make MicroLog read the configuration file and 
monitor it for future changes. This should probably
be done as part of the startup of your app:

	static void Main(string[] args) {
		// Tell microlog to read and monitor the config file for changes
		MicroLogLogger.Output.MonitorConfigFile();
		...
	}

You can also configure MicroLog without using any config files:

	// log everything at level Info or above to the console
	MicroLogLogger.Output.AddFixedTarget(new ConsoleTarget(MicrologLevel.Info, MicrologLayout.Default), false);

	// log everything to file
	MicroLogLogger.Output.AddFixedTarget(new FileTarget(MicroLogLevel.Debug, MicroLogLayout.Default, new MicroLogLayout("testfile.log")), false);


Async Writing
=============
Continously writing and flushing log events to files on disc will eat up a fair amount
disk I/O and CPU, and is not appropriate for high-performance applications.

That's why MicroLog supports async writing of events to any target.

When a target is marked as async, events aren't written to the
target in the thread that logs the event. Instead, the event is
stored in a queue which is periodically flushed to the target by a
seperate thread. 

This lets the primary thread continue without any waiting, and has the
nice benefit of grouping disk I/O into large chunks.

You can mark targets as async from the AddFixedTarget(..) method or directly
in the XML config by adding async="true" to the &lt;target /&gt; element.

	 <target type="console"	minlevel="debug" async="true" />

Console Target
--------------
The console target simply writes log messages to the console in full technicolor.

File Target
-----------
The file target let's you write log messages between rolling files.

For instance, if you configure it like so:

	<target type="file"	file="Logs/$starttime.txt" />

It will generate a new logfile each time the app is started based on the layout from the file attribute.

You can also easily have one log file pr. level:

	<target type="file"	file="Logs/$level.txt" />

See the Layouts section for more information about the posibilities.

Layouts
=======
Layouts let you describe how log events are converted into strings.
You can think of layouts as optimized replacements being
performed on the input string.

For example, this ConsoleTarget uses a custom layout for event printing:

	<target type="console" layout="($level) $logger $message $exception" />

Layouts are also used in the FileTarget to define the output
filename. This can be used to make one FileTarget output to
multiple files.

For instance, you could create one log file per level: 

	<target type="file"	file="log_$level.txt" />

Or one file every time the app starts.

	<target type="file"	file="log_$starttime.txt" />

There are a few layout parts built in:

<table border="1">
	<tr>
		<th>Part</th>
		<th>Part</th>
	</tr>
	<tr><td>$version</td><td>Any string you assign to the MicroLogLayout.Version property</td></tr>
	<tr><td>$time</td><td>The current time</td></tr>
	<tr><td>$starttime</td><td>The time the current application was started</td></tr>
	<tr><td>$message</td><td>The message of the current log event</td></tr>
	<tr><td>$level</td><td>The level of the current log event</td></tr>
	<tr><td>$logger</td><td>The name of the logger for the current log event</td></tr>
	<tr><td>$exception</td><td>Any exception associated with the current log event</td></tr>
</table>

As with everything else in MicroLog, it's easy to define and add custom layout parts:

	// define a class for the part
	public class MachineName : MicroLogLayout.Part{ 
		public override void Render(MicroLogEvent evt, StringBuilder output) { 
			output.Append(Environment.MachineName);
		}
	}

	// register the part
	MicroLogLayout.RegisterPartFactory("machinename", () => new MachineName() );

Custom Targets
================
MicroLog only ships with two logging targets: ConsoleTarget and FileTarget. 

If you want to log to any other type of target, you'll have to
create a MicrologTarget yourself. However, this is very, very easy.

Simply create a class that inherits from the MicroLogTarget, and
register it with either MicroLogOutput.AddFixedTarget(..) or MicroLogTarget.AddConfigTarget(..).

Example:

	private class MyCustomTarget : MicroLogTarget {
		public MyCustomTarget(MicroLogLevel minimumLevel, MicroLogLayout layout) : base(minimumLevel, layout) { }
		protected override void Write(MicroLogEvent evt, bool flushAfterWrite) {
			Console.WriteLine(("MyCustomTarget: [" + evt.Logger + "] ").PadRight(50) + evt.Message);
		}
	}

Take a look at FileTarget or ConsoleTarget for more detailed examples.

Performance Optimizations
=========================
Use the IsLevelEnabled methods guard against unnesscary work:

	if(logger.IsDebugEnabled) logger.Debug("I'm debuggin here:" + someDebugValue()) 

This is mostly useful for debug logging which you don't always leave
enabled. If you are using the configuration file monitoring, you can have
fast performance AND be able to enable debug logging at runtime using this method.

Custom Loggers
==============
Sometimes you need the ability to easily swap out or disable the entire
logging framework across your app. 

MicroLog supports this having the Logger be an abstract base
class with a simple Factory creation method. 

By default, loggers created are of the type 'MicroLogLogger' which
uses the entire MicroLogging framework, but you can also use any
other type deriving from Logger as the logger type by setting the
factory on the Logger class.

For instance, if you'd like to disable all logging in the most
performant way across your entire app simply use the NowhereLogger:

	Logger.SetFactory( new NowhereLogger.Factory() )

You can also create and use your own Logger class by creating a custom
factory that returns your type. See the ConsoleLogger for an example.

Cross App Domain Logging
========================
MicroLog has special support for apps that run across
multiple App Domains: The MicroLogOutput class can be passed by refence between App Domains
(MarshalByReference), letting you use the MicroLogOutput object
from the parent App Domain inside a Child App domain. 

The main advantage of which is that all app domains are then able to
log to the same file.

In order to set this up, simply pass the MicroLogOutput object into
the child App Domain:

	// something like this
	childAppDomain.SetLogging( MicroLogLogger.Output )

And just override the default value inside the child app domain:

	public void SetLogging(MicroLogOutput output) {
		MicroLogLogger.Output = output;
	}

License
=======
Copyright (c) 2012 Oliver Kofoed

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
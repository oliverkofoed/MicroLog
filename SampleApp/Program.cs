using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroLog;

namespace SampleApp{
	public class Program {
		static void Main(string[] args) {
			// first we're going to log without having
			// done any sort of configuration. This 
			// won't result in anything being logged.
			doSomeLogging("1: No Setup.");

			// You can add microlog targets from code.
			MicroLogLogger.Output.AddFixedTarget(new ConsoleTarget(MicroLogLevel.Info, MicroLogLayout.Default), false);
			doSomeLogging("2: Fixed Targets.");

			// You could also just put the configuration
			// in the app.config file. Notice that any
			// targets added with AddFixedTarget aren't
			// removed when you do this.
			MicroLogLogger.Output.MonitorConfigFile();
			doSomeLogging("3: Targets from configuration file.");

			// This is how you add your own custom target as a fixed target
			MicroLogLogger.Output.AddFixedTarget(new MyCustomTarget(MicroLogLevel.Info, MicroLogLayout.Default), false);
			doSomeLogging("4: With custom fixed target.");

			// Or, you can make your target available to configuration files
			MicroLogLogger.Output.AddConfigTarget("mycustomtarget", (minimumLevel, layout, xmlNode) => new MyCustomTarget(minimumLevel, layout) ); 
			doSomeLogging("5: With custom target from config file.");

			// This example completely bypasses the
			// micrologging framework and just creates 
			// very simple console loggers.
			Logger.SetFactory(new ConsoleLogger.Factory());
			doSomeLogging("6: Console Logger.");

			// You can also completely disable logging
			// across your entire app in a very performant
			// way by utilizing the NowhereLogger.
			Logger.SetFactory(new NowhereLogger.NowhereLoggerFactory());
			doSomeLogging("7: NowhereLogger.");

			// keep the app running to let the user view the result
			Console.WriteLine("Done. Press enter to quit.");
			Console.ReadLine();
		}

		private static void doSomeLogging(string heading) {
			Console.WriteLine("\n"+ heading+ "\n");

			// Get a logger and write some stuff
			var logger = Logger.GetClassLogger();
			logger.Debug("I'm debugging");
			logger.Info("I'm informing you");
			logger.Warn("I'm warning you...");
			logger.Error("I'm faulty. I err.");
			logger.Fatal("I failed comletely!");
		}

		private class MyCustomTarget : MicroLogTarget {
			public MyCustomTarget(MicroLogLevel minimumLevel, MicroLogLayout layout) : base(minimumLevel, layout) { }
			protected override void Write(MicroLogEvent evt, bool flushAfterWrite) {
				Console.WriteLine(("MyCustomTarget: [" + evt.Logger + "] ").PadRight(50) + evt.Message);
			}
		}
	}
}

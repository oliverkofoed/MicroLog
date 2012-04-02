using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;

namespace MicroLog {
	[Serializable]
	public class MicroLogEvent {
		public string Logger;
		public string Message;
		public MicroLogLevel Level;
		public string Exception;
	}

	public enum MicroLogLevel : byte{
		Trace,Debug,Info,Warn,Error,Fatal
	}

	public class MicroLogOutput : MarshalByRefObject {
		private Dictionary<string, Func<MicroLogLevel, MicroLogLayout, XmlElement, MicroLogTarget>> configTargets = new Dictionary<string, Func<MicroLogLevel, MicroLogLayout, XmlElement, MicroLogTarget>>();
		private List<MicroLogTarget> fixedTargets = new List<MicroLogTarget>();
		private List<MicroLogTarget> fixedAsyncTargets = new List<MicroLogTarget>();
		private FileInfo configFile;
		private MicroLogTarget[] targets = new MicroLogTarget[0];
		private MicroLogTarget[] asyncTargets = new MicroLogTarget[0];
		private List<MicroLogEvent> asyncList = new List<MicroLogEvent>();
		private FileSystemWatcher watcher = null;
		private int asyncSleepTime = 50;
		public bool IsTraceEnabled = false;
		public bool IsDebugEnabled = false;
		public bool IsInfoEnabled = false;
		public bool IsWarnEnabled = false;
		public bool IsErrorEnabled = false;
		public bool IsFatalEnabled = false;

		public MicroLogOutput() {
			configTargets["console"] = (minLevel, layout, node) => new ConsoleTarget(minLevel, layout);
			configTargets["file"] = (minLevel,layout,node) => new FileTarget(minLevel, layout,  MicroLogTarget.GetLayout(node,"file","") ){
				 LoggerLock = MicroLogTarget.GetAttr(node,"logger",null)	
			};
			configTargets["exceptions"] =(minLevel,layout,node) => new FileTarget(minLevel, layout,  MicroLogTarget.GetLayout(node,"file","") ){
				 ExceptionsOnly = true
			};
		}

		public override object InitializeLifetimeService() { return null; }
		
		public void AddFixedTarget(MicroLogTarget target, bool async) {
			(async ? fixedAsyncTargets : fixedTargets).Add(target);

			var source = (async ? asyncTargets : targets);
			var newTargets = new MicroLogTarget[source.Length+1];
			Array.Copy(source,newTargets, source.Length);
			newTargets[newTargets.Length-1] = target;

			if (async){
				asyncTargets = newTargets;
			}else{
				targets = newTargets;
			}
		}

		public void AddConfigTarget(string name, Func<MicroLogLevel, MicroLogLayout, XmlElement, MicroLogTarget> factory) {
			configTargets[name] = factory;
			if(configFile != null) {
				MonitorConfigFile(configFile);
			}
		}

		public void MonitorConfigFile() {
			MonitorConfigFile(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
		}

		public void MonitorConfigFile(FileInfo configFile) {
			if(configFile.Exists) {
				// close existing watcher
				if(watcher != null) {
					watcher.Dispose();
					watcher = null;
				}

				// parse the config file
				this.configFile = configFile;
				parseConfigFile();

				// setup watcher to wait for changes in file
				watcher = new FileSystemWatcher(configFile.DirectoryName);
				watcher.Filter = configFile.Name;
				watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
				watcher.IncludeSubdirectories = false;
				watcher.Changed += new FileSystemEventHandler((a,b)=>parseConfigFile());
				watcher.Renamed += new RenamedEventHandler((a, b) => parseConfigFile());// renamed is required, because VS.NET writes to a temp file that it renames to actual file
				watcher.Deleted += new FileSystemEventHandler((a, b) => parseConfigFile());
				watcher.Created += new FileSystemEventHandler((a, b) => parseConfigFile());
				watcher.EnableRaisingEvents = true;
			}
		}

		private DateTime lastParse = DateTime.MinValue;
		private void parseConfigFile() {
			// don't parse THAT often
			if((DateTime.Now - lastParse).TotalMilliseconds < 5) { return; }
			lastParse = DateTime.Now;
			
			// read the xml
			var xml = "";
			try{xml = File.ReadAllText(configFile.FullName);}catch{}
			if( xml == "" )return;

			// parse the file
			try {
				List<MicroLogTarget> targets = new List<MicroLogTarget>(fixedTargets);
				List<MicroLogTarget> asyncTargets = new List<MicroLogTarget>(fixedAsyncTargets);

				// load the xml
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				var lowestLevel = MicroLogLevel.Fatal;

				foreach(XmlElement node in doc.SelectNodes("//microlog")) {
					// read async sleep time.
					int.TryParse(MicroLogTarget.GetAttr(node, "asyncflushtime", "50"), out asyncSleepTime);
					if( asyncSleepTime<50 ){
						asyncSleepTime = 50;
					} else if(asyncSleepTime > 20000) {
						asyncSleepTime = 20000;
					}

					foreach(XmlElement target in node.SelectNodes("target")) {
						// find the minimum level
						MicroLogLevel minLevel = MicroLogLevel.Fatal;
						switch(MicroLogTarget.GetAttr(target, "minlevel", "").ToLower()) {
							case "trace":minLevel=MicroLogLevel.Trace;break;
							case "debug": minLevel=MicroLogLevel.Debug;break;
							case "info": minLevel=MicroLogLevel.Info;break;
							case "warn": minLevel=MicroLogLevel.Warn;break;
							case "error": minLevel=MicroLogLevel.Error;break;
						}

						// create the target
						MicroLogTarget t = null;
						Func<MicroLogLevel, MicroLogLayout, XmlElement, MicroLogTarget> factory;
						if( configTargets.TryGetValue( MicroLogTarget.GetAttr(target, "type", ""), out factory ) ){
							t = factory(minLevel,MicroLogTarget.GetLayout(target,"layout",""), target);
						}

						// add the target to the correct list
						var isAsync = MicroLogTarget.GetAttr(node, "async", "false").ToLower() == "true";
						(isAsync ? asyncTargets : targets).Add( t );

						// find the smallest allowed level
						if(minLevel < lowestLevel) {
							lowestLevel = minLevel;
						}
					}
				}

				// save targets
				this.targets = targets.ToArray();
				this.asyncTargets = asyncTargets.ToArray();

				// ensure an async thread is present, if we're async sending...
				if( asyncTargets.Count > 0){
					ensureFlushThread();
				}

				// set the enabled/disabled
				IsTraceEnabled = lowestLevel <= MicroLogLevel.Trace;
				IsDebugEnabled = lowestLevel <= MicroLogLevel.Debug;
				IsInfoEnabled = lowestLevel <= MicroLogLevel.Info;
				IsErrorEnabled = lowestLevel <= MicroLogLevel.Error;
				IsWarnEnabled = lowestLevel <= MicroLogLevel.Warn;
				IsFatalEnabled = lowestLevel <= MicroLogLevel.Fatal;
			} catch(Exception e) {
				Debug.WriteLine("Error parsing config file: " + e.ToString());
				Console.WriteLine("Error parsing config file: " + e.ToString());
			}
		}

		public void Write(MicroLogEvent evt) {
			// queue up event for async targets
			if(asyncTargets.Length>0) {
				lock(this) {
					asyncList.Add(evt);
				}
			}

			// write out for all current targets
			write(targets, evt, true);
		}

		private void write(MicroLogTarget[] targets, MicroLogEvent evt, bool flushAfterWrite) {
			try {
				foreach(var target in targets) {
					target.DoWrite(evt, flushAfterWrite);
				}
			} catch{

			}
		}

		private Thread thread = null;
		private void ensureFlushThread() {
			if(thread == null) {
				thread = new Thread((ThreadStart)delegate {
					while(true) {
						// empty waiting messages
						if(this.asyncList != null) {
							List<MicroLogEvent> events;
							lock(this) {
								events = this.asyncList;
								this.asyncList = new List<MicroLogEvent>(events.Count);
							}

							// write all the events
							for(int i = 0; i != events.Count; i++) {
								write(asyncTargets, events[i], i==events.Count-1);
							}
						}

						Thread.Sleep(asyncSleepTime);
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}
	}

	// stupid .net infrastructure requires this...
	public sealed class ConfigSectionHandler : IConfigurationSectionHandler {
		public object Create(object parent, object configContext, System.Xml.XmlNode section) {
			return new object(); // whatever...
		}
	}
}
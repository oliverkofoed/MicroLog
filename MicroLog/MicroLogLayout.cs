using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MicroLog {
	public class MicroLogLayout {
		public static string Version;
		public static MicroLogLayout Default { get { return new MicroLogLayout(null); } }
		private static DateTime starttime = Process.GetCurrentProcess().StartTime;
		private static Dictionary<string, Func<Part>> partFactories = new Dictionary<string, Func<Part>>();

		public static void RegisterPartFactory(string name, Func<Part> factory) {
			partFactories[name] = factory;
		}

		public abstract class Part {
			public virtual int ExpectedLength { get { return 40; } }
			public abstract void Render(MicroLogEvent evt, StringBuilder output); 
		}

		#region built in parts
		static MicroLogLayout(){
			RegisterPartFactory("time", () => new DateTimePart());
			RegisterPartFactory("starttime", () => new StartTimePart());
			RegisterPartFactory("version", () => new VersionPart());
			RegisterPartFactory("message", () => new MessagePart());
			RegisterPartFactory("logger", () => new LoggerPart());
			RegisterPartFactory("level", () => new LevelPart());
			RegisterPartFactory("exception", () => new ExceptionPart());
		}

		public class StartTimePart : Part{ public override void Render(MicroLogEvent evt, StringBuilder output) { format(output, starttime); } }
		public class DateTimePart : Part{ public override void Render(MicroLogEvent evt, StringBuilder output) { format(output, DateTime.Now); } }
		public class VersionPart : Part{ public override void Render(MicroLogEvent evt, StringBuilder output) { output.Append(Version); } }
		public class MessagePart : Part {
			public override int ExpectedLength { get { return 100; } }
			public override void Render(MicroLogEvent evt, StringBuilder output) { if(evt != null) { output.Append(evt.Message); } } 
		}
		public class LoggerPart : Part{ public override void Render(MicroLogEvent evt, StringBuilder output) { if(evt != null) { output.Append(evt.Logger); } } }
		public class LevelPart : Part{ public override void Render(MicroLogEvent evt, StringBuilder output) { if(evt != null) { output.Append(evt.Level); } } }
		public class ExceptionPart : Part{ public override void Render(MicroLogEvent evt, StringBuilder output) { if(evt != null) { output.Append(evt.Exception); } } }
		public class StringPart : Part{
			private readonly string value;
			public StringPart(string value) { this.value = value; }
			public override void Render(MicroLogEvent evt, StringBuilder output) { output.Append(value); } 
		}
		public class DefaultPart : Part{ 
			public override void Render(MicroLogEvent evt, StringBuilder output) {
				format(output, DateTime.Now);
				output.Append(" ").Append(evt.Level);
				output.Append(" ").Append(evt.Logger);
				output.Append(" ").Append(evt.Message);
				output.Append(" ").Append(evt.Exception);
			} 
		}

		private static void format(StringBuilder output, DateTime datetime) {
			zeroPad(output, datetime.Day);
			output.Append("-");
			zeroPad(output, datetime.Month);
			output.Append(" at ");
			zeroPad(output, datetime.Hour);
			output.Append("h ");
			zeroPad(output, datetime.Minute);
			output.Append("m ");
			zeroPad(output, datetime.Second);
			output.Append("s");
		}

		private static void zeroPad(StringBuilder output, int value) {
			if(value < 10) {
				output.Append("0");
			}
			output.Append(value);
		}
		#endregion

		private Regex splitter = new Regex("\\$([a-z]+)", RegexOptions.Compiled | RegexOptions.Multiline);
		private Part[] parts;
		private int estimatedSize = 0;

		public MicroLogLayout(string layout) {
			// split the layout into an array of parts
			if(!string.IsNullOrEmpty(layout)) {
				parts = Array.ConvertAll( splitter.Split(layout), delegate(string input){
					Func<Part> factory;
					return partFactories.TryGetValue(input, out factory) ? factory() : new StringPart(input);
				});
			} else {
				parts = new Part[] { new DefaultPart() };
			}

			// calculate estimated size
			Array.ForEach(parts, part => estimatedSize += part.ExpectedLength);
		}

		public string Render(MicroLogEvent evt) {
			var output = new StringBuilder(estimatedSize);
			for(int i=0;i!=parts.Length;i++){
				parts[i].Render(evt, output);
			}
			return output.ToString();
		}
	}
}
using System;

namespace MicroLog {
	public class ConsoleTarget : MicroLogTarget{
		public ConsoleTarget(MicroLogLevel minimumLevel, MicroLogLayout layout) : base(minimumLevel, layout) { }
		protected override void Write(MicroLogEvent evt, bool flushAfterWrite) {
			ConsoleColor color;
			switch(evt.Level) {
				case MicroLogLevel.Trace: color=ConsoleColor.DarkGray;break;
				case MicroLogLevel.Debug: color=ConsoleColor.Gray;break;
				case MicroLogLevel.Info: color=ConsoleColor.White;break;
				case MicroLogLevel.Warn: color=ConsoleColor.Yellow;break;
				case MicroLogLevel.Error: color=ConsoleColor.Red;break;
				default: color=ConsoleColor.Red;break;
			}
			var existingColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(Layout.Render(evt));
			Console.ForegroundColor = existingColor;
		}
	}
}
using System;

namespace MicroLog{
	public class ConsoleLogger : Logger {
		public new class Factory : Logger.Factory {
			public Logger Create(string name) {
				return new ConsoleLogger(name);
			}

			public Logger Create(Type type) {
				return new ConsoleLogger(type.FullName);
			}
		}

		private string name;
		public ConsoleLogger() : this(""){}
		public ConsoleLogger(string name) { this.name = name == "" ? "" : name+": "; }

		public override bool IsTraceEnabled { get { return false; } }
		public override bool IsDebugEnabled { get { return false; } }
		public override bool IsInfoEnabled { get { return true; } }
		public override bool IsWarnEnabled { get { return true; } }
		public override bool IsErrorEnabled { get { return true; } }
		public override bool IsFatalEnabled { get { return true; } }

		public override void Trace(string message) {
			print(message, ConsoleColor.DarkGray);
		}

		public override void Debug(string message) {
			print(message, ConsoleColor.Gray);
		}

		public override void Info(string message, params object[] args) {
			print(string.Format(message, args), ConsoleColor.White);
		}

		public override void Warn(string message, params object[] args) {
			print(string.Format(message, args), ConsoleColor.Yellow);
		}

		public override void Error(string message, params object[] args) {
			print(string.Format(message, args), ConsoleColor.Red);
		}

		public override void Fatal(string message, params object[] args) {
			print(string.Format(message, args), ConsoleColor.Red);
		}

		public override void TraceException(string message, Exception e) {
			print(message + "\n" + e.ToString(), ConsoleColor.Gray);
		}

		public override void DebugException(string message, Exception e) {
			print(message + "\n" + e.ToString(), ConsoleColor.Gray);
		}

		public override void InfoException(string message, Exception e, params object[] args) {
			print(message + "\n" + e.ToString(), ConsoleColor.White);
		}

		public override void WarnException(string message, Exception e, params object[] args) {
			print(message + "\n" + e.ToString(), ConsoleColor.Yellow);
		}

		public override void ErrorException(string message, Exception e, params object[] args) {
			print(message + "\n" + e.ToString(), ConsoleColor.Red);
		}

		public override void FatalException(string message, Exception e, params object[] args) {
			print(message + "\n" + e.ToString(), ConsoleColor.Red);
		}

		private void print(string message, ConsoleColor color) {
			var existingColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(name + "" + message);
			Console.ForegroundColor = existingColor;
		}
	}
}
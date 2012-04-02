using System;

namespace MicroLog {
	public class NowhereLogger : Logger {
		public class NowhereLoggerFactory : Logger.Factory {
			public Logger Create(string name) { return new NowhereLogger(); }
			public Logger Create(Type type) { return new NowhereLogger(); }
		}
		public override bool IsTraceEnabled { get { return false; } }
		public override bool IsDebugEnabled { get { return false; } }
		public override bool IsInfoEnabled { get { return false; } }
		public override bool IsWarnEnabled { get { return false; } }
		public override bool IsErrorEnabled { get { return false; } }
		public override bool IsFatalEnabled { get { return false; } }
		public override void Trace(string message) { }
		public override void Debug(string message) { }
		public override void Info(string message, params object[] args) { }
		public override void Warn(string message, params object[] args) { }
		public override void Error(string message, params object[] args) { }
		public override void Fatal(string message, params object[] args) { }
		public override void TraceException(string message, Exception e) { }
		public override void DebugException(string message, Exception e) { }
		public override void InfoException(string message, Exception e, params object[] args) { }
		public override void WarnException(string message, Exception e, params object[] args) { }
		public override void ErrorException(string message, Exception e, params object[] args) { }
		public override void FatalException(string message, Exception e, params object[] args) { }
	}
}
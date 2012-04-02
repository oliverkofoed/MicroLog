using System;

namespace MicroLog{
	public class MicroLogLogger : Logger {
		public new class Factory : Logger.Factory {
			public Logger Create(string name) { return new MicroLogLogger(name); }
			public Logger Create(Type type) { return new MicroLogLogger(type.FullName); }
		}

		private static MicroLogOutput output = new MicroLogOutput();
		public static MicroLogOutput Output {
			get { return output; }
			set {
				output = value;
				refreshEnabled();
			}
		}

		private string name;
		private MicroLogLogger(string name) { this.name = name; }

		#region IsLevelEnabled properties
		// The Is* properties use static variables that are copied
		// in from the containing appdomain once a second
		private static System.Threading.Timer updateTimer = null;
		private static bool isTraceEnabled = false;
		private static bool isDebugEnabled = false;
		private static bool isInfoEnabled = false;
		private static bool isWarnEnabled = false;
		private static bool isErrorEnabled = false;
		private static bool isFatalEnabled = false;
		static MicroLogLogger(){
			updateTimer = new System.Threading.Timer(delegate {
				refreshEnabled();
			}, null, 1000, 1000);
		}
		private static void refreshEnabled() {
			isTraceEnabled = Output.IsTraceEnabled;
			isDebugEnabled = Output.IsDebugEnabled;
			isInfoEnabled = Output.IsInfoEnabled;
			isWarnEnabled = Output.IsWarnEnabled;
			isErrorEnabled = Output.IsErrorEnabled;
			isFatalEnabled = Output.IsFatalEnabled;
		}
		#endregion

		public override bool IsTraceEnabled { get {return isTraceEnabled;} }
		public override bool IsDebugEnabled { get {return isDebugEnabled;} }
		public override bool IsInfoEnabled { get {return isInfoEnabled;} }
		public override bool IsWarnEnabled { get {return isWarnEnabled;} }
		public override bool IsErrorEnabled { get {return isErrorEnabled;} }
		public override bool IsFatalEnabled { get {return isFatalEnabled;} }

		public override void Trace(string message) {
			Output.Write(new MicroLogEvent { Message = message, Level = MicroLogLevel.Trace, Logger = name });
		}

		public override void Debug(string message) {
			Output.Write(new MicroLogEvent { Message = message, Level = MicroLogLevel.Debug, Logger = name });
		}

		public override void Info(string message, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Info, Logger = name });
		}

		public override void Warn(string message, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Warn, Logger = name });
		}

		public override void Error(string message, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Error, Logger = name });
		}

		public override void Fatal(string message, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Fatal, Logger = name });
		}

		public override void TraceException(string message, Exception e) {
			Output.Write(new MicroLogEvent { Message = message, Level = MicroLogLevel.Fatal, Logger = name, Exception=e.ToString() });
		}

		public override void DebugException(string message, Exception e) {
			Output.Write(new MicroLogEvent { Message = message, Level = MicroLogLevel.Fatal, Logger = name, Exception=e.ToString() });
		}

		public override void InfoException(string message, Exception e, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Fatal, Logger = name, Exception=e.ToString() });
		}

		public override void WarnException(string message, Exception e, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Fatal, Logger = name, Exception=e.ToString() });
		}

		public override void ErrorException(string message, Exception e, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Fatal, Logger = name, Exception=e.ToString() });
		}

		public override void FatalException(string message, Exception e, params object[] args) {
			Output.Write(new MicroLogEvent { Message = args==null ||args.Length==0?message:string.Format(message,args), Level = MicroLogLevel.Fatal, Logger = name, Exception=e.ToString() });
		}
	}
}
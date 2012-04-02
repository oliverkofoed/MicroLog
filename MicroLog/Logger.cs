using System;
using System.Diagnostics;

namespace MicroLog {
	public abstract class Logger {
		public interface Factory {
			Logger Create(string name);
			Logger Create(Type type);
		}

		private static Factory factory = new MicroLogLogger.Factory();

		public static void SetFactory(Factory factory) {
			if( factory !=null ){
				Logger.factory = factory;	
			}
		}

		public static Logger Get(string name) {
			return factory.Create(name);
		}

		public static Logger Get(Type type) {
			return factory.Create(type);
		}

		public static Logger GetClassLogger() {
			return Get(new StackTrace().GetFrame(1).GetMethod().DeclaringType);
		}

		//-------------------------------------------------
		// Instance methods below: the logging interface
		//-------------------------------------------------
		public abstract bool IsTraceEnabled { get; }
		public abstract bool IsDebugEnabled { get; }
		public abstract bool IsInfoEnabled { get; }
		public abstract bool IsWarnEnabled { get; }
		public abstract bool IsErrorEnabled { get; }
		public abstract bool IsFatalEnabled { get; }

		/// <summary>
		/// Outputs information only usefull for really verbose debugging. 
		/// Use this method everywhere you please
		/// </summary>
		public abstract void Trace(string message);

		/// <summary>
		/// Outputs information only usefull for debugging. 
		/// Use this method everywhere you please
		/// </summary>
		public abstract void Debug(string message);

		/// <summary>
		/// Log general information about application events and status; 
		/// 
		/// Examples;
		///		- Startup, shutdown, cleanup,
		///		- 100 messages hit.
		/// </summary>
		public abstract void Info(string message, params object[] args);

		/// <summary>
		/// Log a warning. 
		/// 
		/// Use if you encounter an error that shouldn't happen, but that is non-fatal;
		/// something that you can recover from or disregard.
		/// 
		/// Examples;
		///		- Unable to find user
		///		- Timeouts	
		/// </summary>
		public abstract void Warn(string message, params object[] args);

		/// <summary>
		/// Logs an error.
		/// 
		/// Examples;
		///		- Database insert failed.
		///		- Object was null, never should be.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public abstract void Error(string message, params object[] args);

		/// <summary>
		/// Log a fatal error.
		/// 
		/// Examples
		///		- Database doesn't respond
		///		- Unable to load required text files
		///		- No configuration specified
		///		- Important part of application threw exception
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public abstract void Fatal(string message, params object[] args);

		/// <summary>
		/// Outputs information only usefull for really verbose debugging. 
		/// Use this method everywhere you please
		/// </summary>
		public abstract void TraceException(string message, Exception e);

		/// <summary>
		/// Outputs information only usefull for debugging. 
		/// Use this method everywhere you please
		/// </summary>
		public abstract void DebugException(string message, Exception e);

		/// <summary>
		/// Log general information about application events and status; 
		/// 
		/// Examples;
		///		- Startup, shutdown, cleanup,
		///		- 100 messages hit.
		/// </summary>
		public abstract void InfoException(string message, Exception e, params object[] args);

		/// <summary>
		/// Log a warning. 
		/// 
		/// Use if you encounter an error that shouldn't happen, but that is non-fatal;
		/// something that you can recover from or disregard.
		/// 
		/// Examples;
		///		- Unable to find user
		/// </summary>
		public abstract void WarnException(string message, Exception e, params object[] args);

		/// <summary>
		/// Logs an error.
		/// 
		/// Examples;
		///		- Database insert failed.
		///		- Object was null, never should be.
		/// </summary>
		public abstract void ErrorException(string message, Exception e, params object[] args);

		/// <summary>
		/// Log a fatal error.
		/// 
		/// Examples
		///		- Database doesn't respond
		///		- Unable to load required text files
		///		- No configuration specified
		///		- Important part of application threw exception
		/// </summary>
		public abstract void FatalException(string message, Exception e, params object[] args);
	}
}
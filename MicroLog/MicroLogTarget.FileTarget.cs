using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroLog {
	public class FileTarget : MicroLogTarget {
		private MicroLogLayout file;
		public bool ExceptionsOnly = false;
		public string LoggerLock = null;

		public FileTarget(MicroLogLevel minimumLevel, MicroLogLayout layout, MicroLogLayout file) : base(minimumLevel, layout) {
			this.file = file;
		}

		protected override void Write(MicroLogEvent evt, bool flushAfterWrite) {
			if (!ExceptionsOnly || evt.Exception != null) {
				if (LoggerLock == null || evt.Logger == LoggerLock) {
					FileOutputTarget.Write(this.file.Render(null), Layout.Render(evt), flushAfterWrite);
				}
			}
		}
	}

	internal class FileOutputTarget {
		private static System.Threading.Timer closeTimer = null;
		private static Dictionary<string, OpenStream> writers = new Dictionary<string, OpenStream>();
		private class OpenStream {
			public StreamWriter Writer;
			public int AccessCounter = 0;
		}

		static FileOutputTarget(){
			closeTimer = new System.Threading.Timer(delegate {
				List<string> remove = new List<string>();
				lock(writers) {
					foreach(var kv in writers) {
						kv.Value.Writer.Flush();
						if(kv.Value.AccessCounter == 0) {
							remove.Add(kv.Key);
						} else {
							kv.Value.Writer.Flush();
						}
						kv.Value.AccessCounter = 0;
					}
					foreach(var key in remove) {
						writers[key].Writer.Close();
						writers.Remove(key);
					}
				}
			}, null, 20000, 20000);
		}
		
		public static void Write(string file, string text, bool flushAfterWrite) {
			try {
				OpenStream output = get(file);
				if(output != null) {
					output.Writer.WriteLine(text);
					if(flushAfterWrite) {
						output.Writer.Flush();
					}
				}
			} catch {
				; // failure
			}
		}

		private static OpenStream get(string path) {
			OpenStream result;
			if(writers.TryGetValue(path, out result)) {
				result.AccessCounter++;
				return result;
			} else {
				var file = new FileInfo(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), path));
				var dir = file.Directory;
				if(!dir.Exists) {
					dir.Create();
				}
				if(!file.Exists) {
					File.WriteAllText(file.FullName, "");
				}

				lock(writers) {
					return writers[path] = new OpenStream { Writer = new StreamWriter(new FileStream(file.FullName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8, 1024 * 10), AccessCounter = 1 };
				}
			}
		}
	}
}
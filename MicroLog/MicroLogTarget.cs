using System.Xml;

namespace MicroLog {
	public abstract class MicroLogTarget {
		public readonly MicroLogLevel MinimumLevel;
		public readonly MicroLogLayout Layout;

		public MicroLogTarget(MicroLogLevel minimumLevel, MicroLogLayout layout) {
			this.MinimumLevel = minimumLevel;
			this.Layout = layout;
		}

		public void DoWrite(MicroLogEvent evt, bool flushAfterWrite) {
			if(evt.Level >= MinimumLevel) {
				Write(evt, flushAfterWrite);
			}
		}

		protected abstract void Write(MicroLogEvent evt, bool flushAfterWrite);

		public static string GetAttr(XmlElement node, string attribute, string defaultValue) {
			var attr = node.Attributes[attribute];
			return attr == null ? defaultValue : attr.InnerXml;
		}

		public static MicroLogLayout GetLayout(XmlElement node, string attribute, string defaultValue) {
			return new MicroLogLayout(GetAttr(node, attribute, defaultValue));
		}
	}
}
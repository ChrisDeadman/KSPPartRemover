using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Format
{
	public class KspObjectWriter
	{
		public static String ToString (KspObject obj)
		{
			var sb = new StringBuilder ();
			WriteObject (obj, sb, 0);
			return sb.ToString ();
		}

		public static String ToString (KspProperty property)
		{
			var sb = new StringBuilder ();
			WriteProperty (property, sb, 0);
			return sb.ToString ();
		}

		public static String ToString (KspReference reference)
		{
			if (String.IsNullOrEmpty (reference.prefix)) {
				return reference.name;
			}

			return string.Format ("{0}, {1}", reference.prefix, reference.name);
		}

		private static void WriteObject (KspObject obj, StringBuilder sb, int level)
		{
			if (!obj.IsGlobalVessel ()) {
				WriteLine (obj.type, sb, level);
				WriteLine ("{", sb, level);
			}

			foreach (var property in obj.properties) {
				WriteProperty (property, sb, level + (obj.IsGlobalVessel () ? 0 : 1));
			}

			foreach (var child in obj.children) {
				WriteObject (child, sb, level + (obj.IsGlobalVessel () ? 0 : 1));
			}

			if (!obj.IsGlobalVessel ()) {
				WriteLine ("}", sb, level);
			}
		}

		private static void WriteProperty (KspProperty property, StringBuilder sb, int level)
		{
			WriteLine (string.Format ("{0} = {1}", property.name, property.value), sb, level);
		}

		private static void WriteLine (String line, StringBuilder sb, int level)
		{
			for (var i = 0; i < level; i++) {
				sb.Append ("\t");
			}
			sb.AppendLine (line);
		}
	}
}

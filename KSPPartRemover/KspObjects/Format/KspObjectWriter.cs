using System;
using System.Linq;
using System.Text;
using KSPPartRemover.Extension;

namespace KSPPartRemover.KspObjects.Format
{
	public class KspObjectWriter
	{
		public static String ToString (KspObject obj)
		{
			var sb = new StringBuilder ();
			WriteObject (obj, sb, 0);
			return sb.ToString ();
		}

		private static void WriteObject (KspObject obj, StringBuilder sb, int level)
		{
			if (!obj.IsGlobalObject) {
				WriteLine (obj.Type, sb, level);
				WriteLine ("{", sb, level);
			}

			foreach (var property in obj.Properties) {
				WriteProperty (obj, property, sb, level + (obj.IsGlobalObject ? 0 : 1));
			}

			foreach (var child in obj.Children) {
				WriteObject (child, sb, level + (obj.IsGlobalObject ? 0 : 1));
			}

			if (!obj.IsGlobalObject) {
				WriteLine ("}", sb, level);
			}
		}

		private static void WriteProperty (KspObject obj, KspProperty property, StringBuilder sb, int level)
		{
			var stringProperty = property as KspStringProperty;
			if (stringProperty != null) {
				WriteLine ($"{property.Name} = {stringProperty.Text}", sb, level);
				return;
			}

			var partLinkProperty = property as KspPartLinkProperty;
			if (partLinkProperty != null) {
				var craft = obj.Parent as KspCraftObject;
				var refStr = partLinkProperty.IsIdReference ? craft.IdOfChild (partLinkProperty.Part).ToString () : partLinkProperty.Part.Name;

				if (partLinkProperty.Prefix != null) {
					WriteLine ($"{partLinkProperty.Name} = {partLinkProperty.Prefix}, {refStr}", sb, level);
				} else {
					WriteLine ($"{partLinkProperty.Name} = {refStr}",sb, level);
				}
				return;
			}

			throw new NotSupportedException ($"Property of type '{property.GetType ().Name}' not supported");
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

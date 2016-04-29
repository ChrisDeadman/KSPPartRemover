using System;
using System.Linq;
using System.Text;
using KSPPartRemover.Extension;

namespace KSPPartRemover.KspObjects.Format
{
    public class KspObjectWriter
    {
        public static StringBuilder WriteObject (KspObject obj, StringBuilder sb)
        {
            WriteObject (obj, sb, 0);
            return sb;
        }

        private static void WriteObject (KspObject obj, StringBuilder sb, int lvl)
        {
            if (!obj.IsGlobalObject) {
                WriteLine (obj.Type, sb, lvl);
                WriteLine ("{", sb, lvl);
            }

            foreach (var property in obj.Properties) {
                WriteProperty (obj, property, sb, lvl + (obj.IsGlobalObject ? 0 : 1));
            }

            foreach (var child in obj.Children) {
                WriteObject (child, sb, lvl + (obj.IsGlobalObject ? 0 : 1));
            }

            if (!obj.IsGlobalObject) {
                WriteLine ("}", sb, lvl);
            }
        }

        private static void WriteProperty (KspObject obj, KspProperty property, StringBuilder sb, int lvl)
        {
            var stringProperty = property as KspStringProperty;
            if (stringProperty != null) {
                WriteLine ($"{property.Name} = {stringProperty.Text}", sb, lvl      );
                return;
            }

            var partLinkProperty = property as KspPartLinkProperty;
            if (partLinkProperty != null) {
                var craft = obj.Parent as KspCraftObject;
                var refStr = partLinkProperty.IsIdReference ? craft.IdOfChild (partLinkProperty.Part).ToString () : partLinkProperty.Part.Name;

                if (partLinkProperty.Prefix != null) {
                    WriteLine ($"{partLinkProperty.Name} = {partLinkProperty.Prefix}, {refStr}", sb, lvl); 
                } else {
                    WriteLine ($"{partLinkProperty.Name} = {refStr}",sb, lvl     );
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

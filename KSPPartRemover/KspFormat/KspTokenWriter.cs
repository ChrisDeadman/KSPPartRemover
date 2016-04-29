using System;
using System.Text;
using System.Collections.Generic;

namespace KSPPartRemover.KspFormat
{
    public class KspTokenWriter
    {
        public static StringBuilder WriteToken (KspToken token, StringBuilder sb)
        {
            WriteToken (token, sb, 0);
            return sb;
        }

        private static void WriteToken (KspToken token, StringBuilder sb, int lvl)
        {
            if (!token.IsGlobalToken ()) {
                WriteLine (token.Name, sb, lvl);
                WriteLine ("{", sb, lvl);
            }

            var contentLevel = (token.IsGlobalToken () ? lvl : lvl + 1);

            foreach (var a in token.Attributes) {
                WriteAttribute (a, sb, contentLevel);
            }

            foreach (var t in token.Tokens) {
                WriteToken (t, sb, contentLevel);
            }

            if (!token.IsGlobalToken ()) {
                WriteLine ("}", sb, lvl);
            }
        }

        private static void WriteAttribute (KeyValuePair<String, String> attribute, StringBuilder sb, int lvl)
        {
            WriteLine ($"{attribute.Key} = {attribute.Value}", sb, lvl   );
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

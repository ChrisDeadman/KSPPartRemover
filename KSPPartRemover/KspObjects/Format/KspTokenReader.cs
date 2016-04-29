using System;
using System.Linq;
using System.Collections.Generic;

namespace KSPPartRemover.KspObjects.Format
{
    public class KspTokenReader
    {
        public static KspToken ReadToken (String text)
        {
            var lines = text.Split (new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select (str => str.TrimStart ()).ToArray ();

            KspToken token;
            ReadToken (lines, 0, out token);
            return token;
        }

        private static int ReadToken (String[] lines, int index, out KspToken token)
        {
            if (lines.Length <= (index + 1)) {
                token = null;
                return index;
            }

            var name = lines [index].Trim ();
            var isGlobalToken = name.Contains ("=");
            var attributes = new List<KeyValuePair<String, String>> ();
            var tokens = new List<KspToken> ();

            if (!isGlobalToken) {
                index++;
                if (lines [index++] != "{") {
                    throw new FormatException ();
                }
            }

            index = ReadAttributes (lines, index, attributes.Add);
            index = ReadTokens (lines, index, tokens.Add);

            if ((lines.Length > index) && lines [index++] != "}") {
                throw new FormatException ();
            }

            token = isGlobalToken
                ? KspGlobalTokenExtension.CreateGlobalToken (attributes, tokens)
                : new KspToken (name, attributes, tokens);

            return index;
        }

        private static int ReadTokens (String[] lines, int index, Action<KspToken> addToken)
        {
            while (lines.Length > index) {
                if (lines [index] == "}") {
                    break;
                }

                KspToken token;
                index = ReadToken (lines, index, out token);

                if (token != null) {
                    addToken (token);
                } else {
                    break;
                }
            }

            return index;
        }

        private static int ReadAttributes (String[] lines, int index, Action<KeyValuePair<String, String>> addAttribute)
        {
            while (lines.Length > index) {
                var keyValue = lines [index].Split ('=');

                if (keyValue.Length == 2) {
                    addAttribute (new KeyValuePair<String, String> (keyValue [0].Trim (), keyValue [1].Trim ()));
                } else {
                    break;
                }

                index++;
            }

            return index;
        }
    }
}

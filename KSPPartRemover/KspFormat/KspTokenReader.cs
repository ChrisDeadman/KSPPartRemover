using System;
using System.Linq;
using System.Collections.Generic;

namespace KSPPartRemover.KspFormat
{
    public class KspTokenReader
    {
        private static readonly KspToken emptyToken = new KspToken ("", new KeyValuePair<String, String>[0], new KspToken[0]);

        public static KspToken ReadToken (String text)
        {
            var lines = text.Split (new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select (str => str.TrimStart ()).ToArray ();

            KspToken token;
            ReadToken (lines, 0, out token);
            return token ?? emptyToken;
        }

        private static int ReadToken (String[] lines, int index, out KspToken token)
        {
            if ((index + 1) >= lines.Length) {
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

            if (!isGlobalToken) {
                if (index >= lines.Length || lines [index++] != "}") {
                    throw new FormatException ();
                }
                token = new KspToken (name, attributes, tokens);
            } else {
                token = KspTokenGlobalExtension.CreateGlobalToken (attributes, tokens);
            }

            return index;
        }

        private static int ReadTokens (String[] lines, int index, Action<KspToken> addToken)
        {
            while (index < lines.Length) {
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
            while (index < lines.Length) {
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

using System;
using System.Linq;
using System.Collections.Generic;

namespace KSPPartRemover.KspFormat
{
    public class KspTokenReader
    {
        private static readonly KspToken emptyToken = new KspToken("", new KeyValuePair<String, String>[0], new KspToken[0]);

        public static KspToken ReadToken(String text)
        {
            var lines = text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(str => str.TrimStart())
                .ToArray();

            // check if this is a global token
            KeyValuePair<String, String>? attribute;
            ReadAttribute(lines, 0, out attribute);
            var isGlobalToken = (attribute != null);

            // read the token and all of its children
            KspToken token;
            ReadToken(lines, 0, isGlobalToken, out token);
            return token ?? emptyToken;
        }

        private static int ReadAttribute(String[] lines, int index, out KeyValuePair<String, String>? attribute)
        {
            index = SkipEmptyLinesAndComments(lines, index);
            if (index >= lines.Length) { // EOF
                attribute = null;
                return index;
            }

            var line = lines[index];
            var equalsSignIdx = line.IndexOf('=');
            if (equalsSignIdx > 0) {
                var key = line.Substring(0, equalsSignIdx).Trim();
                var value = line.Substring(equalsSignIdx + 1).Trim();
                attribute = new KeyValuePair<String, String>(key, value);
                return index + 1;
            }

            attribute = null;
            return index;
        }

        private static int ReadToken(String[] lines, int index, bool isGlobalToken, out KspToken token)
        {
            // read token start if not a global token
            string name = null;
            if (!isGlobalToken) {
                index = TryReadTokenStart(lines, index, out name);
                if (name == null) {
                    token = null;
                    return index;
                }
            }

            var attributes = new List<KeyValuePair<String, String>>();
            var children = new List<KspToken>();

            while (index < lines.Length) {
                // read token end
                bool endFound;
                index = TryReadTokenEnd(lines, index, out endFound);
                // token end found -> return token
                if (endFound) {
                    if (isGlobalToken) {
                        throw new FormatException("Unexpected token end found!");
                    }
                    token = new KspToken(name, attributes, children);
                    return index;
                }

                // read next attribute
                KeyValuePair<String, String>? attribute;
                index = ReadAttribute(lines, index, out attribute);
                if (attribute != null) {
                    attributes.Add(attribute.Value);
                    continue;
                }

                // read next child
                KspToken childToken;
                index = ReadToken(lines, index, false, out childToken);
                if (childToken != null) {
                    children.Add(childToken);
                    continue;
                }

                // unknown stuff -> just ignore
                index++;
            }

            if (!isGlobalToken) {
                throw new FormatException("No token end found!");
            }

            token = KspTokenGlobalExtension.CreateGlobalToken(attributes, children);
            return index;
        }

        private static int TryReadTokenStart(String[] lines, int index, out string name)
        {
            index = SkipEmptyLinesAndComments(lines, index);
            if (index >= lines.Length) { // EOF
                name = null;
                return index;
            }

            var bracketStart = lines[index].IndexOf('{');
            int bracketLineIdx;

            // bracket on same line
            if (bracketStart >= 0) {
                name = lines[index].Substring(0, bracketStart).Trim();
                bracketLineIdx = index;
            }
            // bracket on next line
            else {
                name = lines[index].Trim();
                bracketStart = 0;
                bracketLineIdx = SkipEmptyLinesAndComments(lines, index + 1);
                if (bracketLineIdx >= lines.Length) { // EOF
                    name = null;
                    return bracketLineIdx;
                }
            }

            // token start
            var tokenStart = lines[bracketLineIdx].Substring(bracketStart).TrimStart();
            if (tokenStart.StartsWith('{')) {
                // more than just token start -> reinsert rest of line
                var remaining = tokenStart.Substring(1);
                if (remaining.Trim().Length > 0) {
                    lines[bracketLineIdx] = remaining;
                    return bracketLineIdx;
                }
                return bracketLineIdx + 1;
            }

            // not a token start
            name = null;
            return index;
        }

        private static int TryReadTokenEnd(String[] lines, int index, out bool found)
        {
            index = SkipEmptyLinesAndComments(lines, index);
            if (index >= lines.Length) { // EOF
                found = false;
                return index;
            }

            // token end
            var tokenEnd = lines[index].TrimStart();
            if (tokenEnd.StartsWith('}')) {
                found = true;
                // more than just token end -> reinsert rest of line
                var remaining = tokenEnd.Substring(1);
                if (remaining.Trim().Length > 0) {
                    lines[index] = remaining;
                    return index;
                }
                return index + 1;
            }

            // not a token end
            found = false;
            return index;
        }

        private static int SkipEmptyLinesAndComments(String[] lines, int index)
        {
            while (index < lines.Length) {
                var line = lines[index].Trim();
                if ((line.Length > 0) && !line.StartsWith("//")) {
                    break;
                }
                index++;
            }
            return index;
        }
    }
}

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

            KspToken token;
            ReadToken(lines, 0, out token);
            return token ?? emptyToken;
        }

        private static int ReadToken(String[] lines, int index, out KspToken token)
        {
            // EOF
            index = SkipEmptyLinesAndComments(lines, index);
            if (index >= lines.Length) {
                token = null;
                return index;
            }

            // read token start
            string name;
            index = TryReadTokenStart(lines, index, true, out name);

            // check if global token
            var isGlobalToken = false;
            if (name == null) {
                name = lines[index];
                if (name.Contains("=")) {
                    isGlobalToken = true;
                } else {
                    throw new FormatException();
                }
            }

            var attributes = new List<KeyValuePair<String, String>>();
            var tokens = new List<KspToken>();

            index = ReadAttributes(lines, index, attributes.Add);
            index = ReadTokens(lines, index, tokens.Add);

            if (!isGlobalToken) {
                bool endFound;
                index = TryReadTokenEnd(lines, index, true, out endFound);
                if (!endFound) {
                    throw new FormatException();
                }
                token = new KspToken(name, attributes, tokens);
            } else {
                token = KspTokenGlobalExtension.CreateGlobalToken(attributes, tokens);
            }

            return index;
        }

        private static int ReadTokens(String[] lines, int index, Action<KspToken> addToken)
        {
            while (index < lines.Length) {
                // stop on token end (but do not consume line)
                bool endFound;
                TryReadTokenEnd(lines, index, false, out endFound);
                if (endFound) {
                    break;
                }

                KspToken token;
                index = ReadToken(lines, index, out token);

                if (token != null) {
                    addToken(token);
                }
            }

            return index;
        }

        private static int ReadAttributes(String[] lines, int index, Action<KeyValuePair<String, String>> addAttribute)
        {
            while (index < lines.Length) {
                var line = lines[index];
                var splitPoint = line.IndexOf('=');

                if (splitPoint > 0) {
                    var key = line.Substring(0, splitPoint).Trim();
                    var value = line.Substring(splitPoint + 1).Trim();
                    addAttribute(new KeyValuePair<String, String>(key, value));
                } else {
                    // stop on token end
                    bool endFound;
                    TryReadTokenEnd(lines, index, false, out endFound);
                    if (endFound) {
                        break;
                    }
                    // stop on next token (child token)
                    string tokenName;
                    TryReadTokenStart(lines, index, false, out tokenName);
                    if (tokenName != null) {
                        break;
                    }
                    // simply ignore other stuff
                }

                index++;
            }

            return index;
        }

        private static int TryReadTokenStart(String[] lines, int index, bool reinsert, out string name)
        {
            // EOF
            index = SkipEmptyLinesAndComments(lines, index);
            if (index >= lines.Length) {
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
                bracketLineIdx = index + 1;
                // EOF
                index = SkipEmptyLinesAndComments(lines, index);
                if (index >= lines.Length) {
                    name = null;
                    return index;
                }
            }

            // token start
            var tokenStart = lines[bracketLineIdx].Substring(bracketStart).TrimStart();
            if (tokenStart.StartsWith('{')) {
                // more than just token start -> reinsert rest of line
                if (reinsert) {
                    var remaining = tokenStart.Substring(1);
                    if (remaining.Trim().Length > 0) {
                        lines[bracketLineIdx] = remaining;
                        return bracketLineIdx;
                    }
                }
                return bracketLineIdx + 1;
            }

            // not a token start
            name = null;
            return index;
        }

        private static int TryReadTokenEnd(String[] lines, int index, bool reinsert, out bool found)
        {
            // EOF
            index = SkipEmptyLinesAndComments(lines, index);
            if (index >= lines.Length) {
                found = false;
                return index;
            }

            // token end
            var tokenEnd = lines[index].TrimStart();
            if (tokenEnd.StartsWith('}')) {
                found = true;
                // more than just token end -> reinsert rest of line
                if (reinsert) {
                    var remaining = tokenEnd.Substring(1);
                    if (remaining.Trim().Length > 0) {
                        lines[index] = remaining;
                        return index;
                    }
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

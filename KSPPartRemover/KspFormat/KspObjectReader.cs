using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.KspFormat
{
    public class KspObjectReader
    {
        public static KspObject ReadObject (KspToken token)
        {
            var tokenMapping = new Dictionary<KspObject, KspToken> ();

            var objectTree = ReadObjectTree (token, tokenMapping.Add);

            var allObjects = FlattenObjectTree (objectTree).ToArray ();

            foreach (var obj in allObjects) {
                ReadStringProperties (obj, tokenMapping [obj].Attributes.ToList ());
            }

            foreach (var obj in allObjects) {
                ReadPartLinkProperties (obj, tokenMapping [obj].Attributes.ToList ());
            }

            return objectTree;
        }

        private static KspObject ReadObjectTree (KspToken token, Action<KspObject, KspToken> addTokenMapping)
        {
            KspObject obj;

            // Treat global tokens as craft object -> needed for .craft file support
            var type = token.IsGlobalToken () ? KspCraftObject.TypeId : token.Name;

            switch (type) {
            case KspCraftObject.TypeId:
                obj = new KspCraftObject (token.IsGlobalToken ());
                break;
            case KspPartObject.TypeId:
                obj = new KspPartObject (token.IsGlobalToken ());
                break;
            default:
                obj = new KspObject (token.Name, token.IsGlobalToken ());
                break;
            }

            foreach (var t in token.Tokens) {
                obj.AddChild (ReadObjectTree (t, addTokenMapping));
            }

            addTokenMapping (obj, token);
            return obj;
        }

        private static IEnumerable<KspObject> FlattenObjectTree (KspObject objTree)
        {
            yield return objTree;

            foreach (var child in objTree.Children) {
                foreach (var obj in FlattenObjectTree (child)) {
                    yield return obj;
                }
            }
        }

        private static void ReadStringProperties (KspObject obj, List<KeyValuePair<String, String>> attributes)
        {
            foreach (var attribute in attributes) {
                if (IsStringAttribute (obj, attribute)) {
                    obj.InsertProperty (attributes.IndexOf (attribute), new KspStringProperty (attribute.Key, attribute.Value));
                }
            }
        }

        private static void ReadPartLinkProperties (KspObject obj, List<KeyValuePair<String, String>> attributes)
        {
            foreach (var attribute in attributes) {
                if (IsPartLinkAttribute (obj, attribute)) {
                    obj.InsertProperty (attributes.IndexOf (attribute), ReadPartLinkProperty (obj, attribute));
                }
            }
        }

        private static bool IsStringAttribute (KspObject obj, KeyValuePair<String, String> attribute) => !IsPartLinkAttribute (obj, attribute);

        private static bool IsPartLinkAttribute (KspObject obj, KeyValuePair<String, String> attribute)
        {
            if (!(obj.Parent is KspCraftObject)) {
                return false;
            }

            var isPartLinkType = attribute.Key.Equals (KspPartLinkProperty.Types.Link) ||
                                 attribute.Key.Equals (KspPartLinkProperty.Types.Parent) ||
                                 attribute.Key.Equals (KspPartLinkProperty.Types.Sym) ||
                                 attribute.Key.Equals (KspPartLinkProperty.Types.SrfN) ||
                                 attribute.Key.Equals (KspPartLinkProperty.Types.AttN);
            if (!isPartLinkType) {
                return false;
            }

            var isSupportedPartLinkType = !attribute.Value.Contains(",Null_0");
            return isSupportedPartLinkType;
        }

        private static KspPartLinkProperty ReadPartLinkProperty (KspObject obj, KeyValuePair<String, String> attribute)
        {
            String prefix = null;
            String reference = attribute.Value;
            String postfix = null;

            var tokens = attribute.Value.Split(',').ToList();

            // at least prefix and reference are present
            if (tokens.Count >= 2) {
                prefix = tokens[0].Trim();
                reference = tokens[1].Trim();

                // new attN format needs special parsing
                if (attribute.Key.Equals(KspPartLinkProperty.Types.AttN) && tokens[1].Contains('|')) {
                    var firstPostfixSep = tokens[1].IndexOf('|');
                    var postfixStart = tokens[1].LastIndexOf("_", firstPostfixSep);
                    reference = tokens[1].Substring(0, postfixStart).Trim();
                    postfix = tokens[1].Substring(postfixStart);
                    if (tokens.Count >= 3) {
                        postfix += $",{String.Join(",", tokens.Skip(2))}";
                    }
                    // attN = top,command-375-biconic-1_4293284166_0|0.189999998|0_0|1|0_0|0.189999998|0_0|1|0
                }
                // simply add postfixes for other types
                else if (tokens.Count >= 3) {
                    postfix = $",{String.Join(",", tokens.Skip(2))}";
                }
            }

            int id;
            var isIdReference = int.TryParse (reference, out id);

            var part = isIdReference
                ? obj.Parent.Child<KspPartObject> (id)
                : obj.Parent.Children<KspPartObject> ().FirstOrDefault (p => p.Name.Equals (reference));
            
            return new KspPartLinkProperty (attribute.Key, prefix, part, postfix, isIdReference);
        }
    }
}

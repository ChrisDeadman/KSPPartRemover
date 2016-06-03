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

            return
            attribute.Key.Equals (KspPartLinkProperty.Types.Link) ||
            attribute.Key.Equals (KspPartLinkProperty.Types.Parent) ||
            attribute.Key.Equals (KspPartLinkProperty.Types.Sym) ||
            attribute.Key.Equals (KspPartLinkProperty.Types.SrfN) ||
            attribute.Key.Equals (KspPartLinkProperty.Types.AttN);
        }

        private static KspPartLinkProperty ReadPartLinkProperty (KspObject obj, KeyValuePair<String, String> attribute)
        {
            var prefixValue = attribute.Value.Split (',').Select (s => s.Trim ()).ToArray ();
            var prefix = (prefixValue.Length == 2) ? prefixValue [0] : null;
            var reference = (prefixValue.Length == 2) ? prefixValue [1] : attribute.Value;

            int id;
            var isIdReference = int.TryParse (reference, out id);

            var part = isIdReference
                ? obj.Parent.Child<KspPartObject> (id)
                : obj.Parent.Children<KspPartObject> ().FirstOrDefault (p => p.Name.Equals (reference));
            
            return new KspPartLinkProperty (attribute.Key, prefix, part, isIdReference);
        }
    }
}

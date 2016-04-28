using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.Extension;

namespace KSPPartRemover.KspObjects.Format
{
    public class KspObjectReader
    {
        public static KspObject ReadObject (String text)
        {
            var lines = text.Split (new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select (str => str.TrimStart ()).ToArray ();

            KspProtoObject rootObjProto;
            ReadProtoObject (lines, 0, out rootObjProto);
            rootObjProto.ResolveChildren ();
            rootObjProto.ResolveProperties<KspStringProperty> ();
            rootObjProto.ResolveProperties<KspPartLinkProperty> ();

            return rootObjProto.Obj;
        }

        public static TProperty ReadProperty<TProperty> (KspObject obj, String text) where TProperty : KspProperty
        {
            var nameValue = text.Split ('=').Select (s => s.Trim ()).ToArray ();
            if (nameValue.Length != 2) {
                return null;
            }

            var name = nameValue [0];
            var value = nameValue [1];

            if (IsPartLinkProperty (obj, name, value)) {
                return typeof(TProperty).Equals (typeof(KspPartLinkProperty)) ? ReadPartLinkProperty (obj, name, value) as TProperty : null;
            }

            return new KspStringProperty (name, value) as TProperty;
        }

        private static bool IsPartLinkProperty (KspObject obj, String name, String value)
        {
            if (!(obj.Parent is KspCraftObject)) {
                return false;
            }

            return name.Equals ("link") || name.Equals ("parent") || name.Equals ("sym") || name.Equals ("srfN") || name.Equals ("attN");
        }

        private static KspPartLinkProperty ReadPartLinkProperty (KspObject obj, String name, String value)
        {
            var prefixValue = value.Split (',').Select (s => s.Trim ()).ToArray ();
            var prefix = (prefixValue.Length == 2) ? prefixValue [0] : null;
            var reference = (prefixValue.Length == 2) ? prefixValue [1] : value;
            int id;
            var isIdReference = int.TryParse (reference, out id);
            var craft = obj.Parent as KspCraftObject;
            var part = isIdReference
                ? craft.Child<KspPartObject> (id)
                : craft.Children<KspPartObject> ().FirstOrDefault (p => p.Name.Equals (reference));
            
            return new KspPartLinkProperty (name, prefix, part, isIdReference);
        }

        private static int ReadProtoObject (String[] lines, int index, out KspProtoObject result)
        {
            if (lines.Length <= (index + 1)) {
                result = null;
                return index;
            }

            var type = KspCraftObject.TypeId;
            var isGlobalObject = lines [index].Contains ("=");
            if (!isGlobalObject) {
                type = lines [index++].Trim ();
                if (lines [index++] != "{") {
                    throw new FormatException ();
                }
            }

            KspObject obj;

            switch (type) {
            case KspCraftObject.TypeId:
                obj = new KspCraftObject (isGlobalObject);
                break;
            case KspPartObject.TypeId:
                obj = new KspPartObject (isGlobalObject);
                break;
            default:
                obj = new KspObject (type, isGlobalObject);
                break;
            }

            List<String> properties;
            List<KspProtoObject> children;
            index = ReadProtoProperties (lines, index, out properties);
            index = ReadProtoObjects (lines, index, out children);

            if ((lines.Length > index) && lines [index] != "}") {
                throw new FormatException ();
            }

            result = new KspProtoObject (obj, properties, children);

            return index + 1;
        }

        private static int ReadProtoObjects (String[] lines, int index, out List<KspProtoObject> objects)
        {
            objects = new List<KspProtoObject> ();

            while (lines.Length > index) {
                KspProtoObject obj;

                if (lines [index] == "}") {
                    break;
                }

                var nextIndex = ReadProtoObject (lines, index, out obj);
                if (nextIndex <= index) {
                    break;
                }
                index = nextIndex;

                objects.Add (obj);
            }

            return index;
        }

        private static int ReadProtoProperties (String[] lines, int index, out List<String> properties)
        {
            properties = new List<String> ();

            while (lines.Length > index) {
                var text = lines [index];
                if (text.Split ('=').Length != 2) {
                    break;
                }

                properties.Add (text);
                index++;
            }

            return index;
        }
    }
}

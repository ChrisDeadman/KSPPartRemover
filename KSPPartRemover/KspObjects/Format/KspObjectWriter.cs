using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using KSPPartRemover.Extension;

namespace KSPPartRemover.KspObjects.Format
{
    public class KspObjectWriter
    {
        public static KspToken WriteObject (KspObject obj)
        {
            var attributes = obj.Properties.Select (p => ResolveProperty (obj, p)).ToList ();
            var tokens = obj.Children.Select (WriteObject).ToList ();

            return obj.IsGlobalObject
                ? KspGlobalTokenExtension.CreateGlobalToken (attributes, tokens)
                : new KspToken (obj.Type, attributes, tokens);
        }

        private static KeyValuePair<String, String> ResolveProperty (KspObject obj, KspProperty property)
        {
            var value = "<INVALID>";

            var stringProperty = property as KspStringProperty;
            if (stringProperty != null) {
                value = stringProperty.Text;
            }

            var partLinkProperty = property as KspPartLinkProperty;
            if (partLinkProperty != null) {
                value = ResolvePartLinkString (obj, partLinkProperty);
            }

            return new KeyValuePair<String, String> (property.Name, value);
        }

        private static String ResolvePartLinkString (KspObject obj, KspPartLinkProperty property)
        {
            var craft = obj.Parent as KspCraftObject;

            var sb = new StringBuilder ();

            if (property.Prefix != null) {
                sb.Append ($"{property.Prefix}, ");
            }

            if (property.IsIdReference) {
                sb.Append ($"{craft.IdOfChild (property.Part)}");
            } else {
                sb.Append ($"{property.Part.Name}");
            }

            return sb.ToString ();
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.KspFormat
{
    public class KspObjectWriter
    {
        public static KspToken WriteObject(KspObject obj)
        {
            var attributes = obj.Properties.Select(p => PropertyToAttribute(obj, p)).ToList();
            var tokens = obj.Children.Select(WriteObject).ToList();

            return obj.IsGlobalObject
                ? KspTokenGlobalExtension.CreateGlobalToken(attributes, tokens)
                : new KspToken(obj.Type, attributes, tokens);
        }

        private static KeyValuePair<String, String> PropertyToAttribute(KspObject obj, KspProperty property)
        {
            var value = "<KSPPR_NOT_SUPPORTED>";

            var stringProperty = property as KspStringProperty;
            if (stringProperty != null) {
                value = stringProperty.Text;
            }

            var partLinkProperty = property as KspPartLinkProperty;
            if (partLinkProperty != null) {
                value = ResolvePartLinkString(obj, partLinkProperty);
            }

            return new KeyValuePair<String, String>(property.Name, value);
        }

        private static String ResolvePartLinkString(KspObject obj, KspPartLinkProperty property)
        {
            var craft = obj.Parent as KspCraftObject;

            var sb = new StringBuilder();

            if (property.Prefix != null) {
                sb.Append(property.Prefix);
                sb.Append(", ");
            }

            if (property.IsIdReference) {
                sb.Append(craft.IdOfChild(property.Part));
            } else {
                sb.Append(property.Part.Name);
            }

            if (property.Postfix != null) {
                sb.Append(property.Postfix);
            }

            return sb.ToString();
        }
    }
}

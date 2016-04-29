using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Feature
{
    public class CraftEditor
    {
        public KspCraftObject Craft { get; }

        public CraftEditor (KspCraftObject craft)
        {
            this.Craft = craft;
        }

        public void RemoveParts (IReadOnlyList<KspPartObject> parts)
        {
            foreach (var part in Craft.Children<KspPartObject>().Except(parts)) {
                part.LinkRefs = RemovePartLinks (part.LinkRefs, parts);
                part.ParentRefs = RemovePartLinks (part.ParentRefs, parts);
                part.SymRefs = RemovePartLinks (part.SymRefs, parts);
                part.SrfNRefs = RemovePartLinks (part.SrfNRefs, parts);
                part.AttNRefs = RemovePartLinks (part.AttNRefs, parts);
            }

            foreach (var part in parts) {
                Craft.RemoveChild (part);
            }
        }

        private static IReadOnlyList<KspPartLinkProperty> RemovePartLinks (IReadOnlyList<KspPartLinkProperty> links, IReadOnlyList<KspPartObject> parts) =>
            links.Where (link => !parts.Contains (link.Part)).ToList ();
    }

    public static class CraftEditorExtensions
    {
        public static CraftEditor Edit (this KspCraftObject obj) => new CraftEditor (obj);
    }
}

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
                part.UpdatePartLinks (KspPartLinkProperty.Types.Link, RemovePartLinks (part.PartLinks (KspPartLinkProperty.Types.Link), parts));
                part.UpdatePartLinks (KspPartLinkProperty.Types.Parent, RemovePartLinks (part.PartLinks (KspPartLinkProperty.Types.Parent), parts));
                part.UpdatePartLinks (KspPartLinkProperty.Types.Sym, RemovePartLinks (part.PartLinks (KspPartLinkProperty.Types.Sym), parts));
                part.UpdatePartLinks (KspPartLinkProperty.Types.SrfN, RemovePartLinks (part.PartLinks (KspPartLinkProperty.Types.SrfN), parts));
                part.UpdatePartLinks (KspPartLinkProperty.Types.AttN, RemovePartLinks (part.PartLinks (KspPartLinkProperty.Types.AttN), parts));
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

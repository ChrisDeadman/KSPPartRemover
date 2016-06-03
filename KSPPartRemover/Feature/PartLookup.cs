using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Feature
{
    public class PartLookup
    {
        private readonly KspCraftObject craft;

        public PartLookup (KspCraftObject craft)
        {
            this.craft = craft;
        }

        public IEnumerable<KspPartObject> LookupParts (RegexFilter partFilter)
        {
            int id;
            if (int.TryParse (partFilter.Pattern, out id)) {
                var dependency = craft.Child<KspPartObject> (id);
                return dependency != null ? new[] { dependency } : Enumerable.Empty<KspPartObject> ();
            }

            return partFilter.Apply (craft.Children <KspPartObject> (), part => part.Name);
        }

        public IEnumerable<KspPartObject> LookupSoftDependencies (KspPartObject dependency) =>
            craft.Children<KspPartObject> ().Where (part => HasSoftDependency (part, dependency));

        public IEnumerable<KspPartObject> LookupHardDependencies (KspPartObject dependency)
        {
            var parts = craft.Children<KspPartObject> ().ToList ();
            var dependencies = new HashSet<KspPartObject> ();
            LookupHardDependencies (parts, dependency, dependencies.Add);
            return dependencies;
        }

        private static void LookupHardDependencies (List<KspPartObject> parts, KspPartObject dependency, Func<KspPartObject, bool> addDependency)
        {
            parts.ForEach (currentPart => {
                if (HasHardDependency (currentPart, dependency)) {
                    if (addDependency (currentPart)) {
                        LookupHardDependencies (parts, currentPart, addDependency);
                    }
                }
            });
        }

        private static bool HasSoftDependency (KspPartObject part, KspPartObject dependency)
        {
            Func<IReadOnlyList<KspPartLinkProperty>, bool> hasSoftDependency =
                (refs) => refs.Any (r => Object.Equals (r.Part, dependency));

            return hasSoftDependency (part.PartLinks (KspPartLinkProperty.Types.Link)) ||
            hasSoftDependency (part.PartLinks (KspPartLinkProperty.Types.Parent)) ||
            hasSoftDependency (part.PartLinks (KspPartLinkProperty.Types.Sym)) ||
            hasSoftDependency (part.PartLinks (KspPartLinkProperty.Types.SrfN)) ||
            hasSoftDependency (part.PartLinks (KspPartLinkProperty.Types.AttN));
        }

        private static bool HasHardDependency (KspPartObject part, KspPartObject dependency)
        {
            Func<IReadOnlyList<KspPartLinkProperty>, bool> hasHardDependency =
                (refs) => refs.Count > 0 && refs.All (r => Object.Equals (r.Part, dependency));
            
            return hasHardDependency (part.PartLinks (KspPartLinkProperty.Types.Parent)) ||
            hasHardDependency (part.PartLinks (KspPartLinkProperty.Types.Sym)) ||
            hasHardDependency (part.PartLinks (KspPartLinkProperty.Types.SrfN)) ||
            hasHardDependency (part.PartLinks (KspPartLinkProperty.Types.AttN));
        }
    }
}

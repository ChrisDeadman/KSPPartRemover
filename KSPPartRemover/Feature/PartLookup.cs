using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Feature
{
    public class PartLookup
    {
        public static IEnumerable<KspPartObject> EvaluateSoftDependencies (KspCraftObject craft, KspPartObject dependency) =>
            craft.Children<KspPartObject> ().Where (part => HasSoftDependency (part, dependency));

        private static bool HasSoftDependency (KspPartObject part, KspPartObject dependency)
        {
            Func<IReadOnlyList<KspPartLinkProperty>, bool> hasSoftDependency =
                (refs) => refs.Any (r => Object.Equals (r.Part, dependency));

            return hasSoftDependency (part.LinkRefs) ||
            hasSoftDependency (part.ParentRefs) ||
            hasSoftDependency (part.SymRefs) ||
            hasSoftDependency (part.SrfNRefs) ||
            hasSoftDependency (part.AttNRefs);
        }

        public static IEnumerable<KspPartObject> EvaluateHardDependencies (KspCraftObject craft, KspPartObject dependency)
        {
            var parts = craft.Children<KspPartObject> ().ToList ();
            var dependencies = new HashSet<KspPartObject> ();
            EvaluateHardDependencies (parts, dependency, dependencies.Add);
            return dependencies;
        }

        private static void EvaluateHardDependencies (List<KspPartObject> parts, KspPartObject dependency, Func<KspPartObject, bool> addDependency)
        {
            parts.ForEach (currentPart => {
                if (HasHardDependency (currentPart, dependency)) {
                    if (addDependency (currentPart)) {
                        EvaluateHardDependencies (parts, currentPart, addDependency);
                    }
                }
            });
        }

        private static bool HasHardDependency (KspPartObject part, KspPartObject dependency)
        {
            Func<IReadOnlyList<KspPartLinkProperty>, bool> hasHardDependency =
                (refs) => refs.Count > 0 && refs.All (r => Object.Equals (r.Part, dependency));
            
            return hasHardDependency (part.ParentRefs) ||
            hasHardDependency (part.SymRefs) ||
            hasHardDependency (part.SrfNRefs) ||
            hasHardDependency (part.AttNRefs);
        }
    }
}

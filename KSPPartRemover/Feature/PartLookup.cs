using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspObjects;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Feature
{
	public class PartLookup
	{
		public static IEnumerable<KspPartObject> EvaluateSoftDependencies (KspCraftObject craft, KspPartObject dependency) =>
			EvaluateDependencies (craft, dependency, HasSoftDependency);

		public static IEnumerable<KspPartObject> EvaluateHardDependencies (KspCraftObject craft, KspPartObject dependency) =>
			EvaluateDependencies (craft, dependency, HasHardDependency);

		private static IEnumerable<KspPartObject> EvaluateDependencies (KspCraftObject craft, KspPartObject dependency, Func<KspPartObject, KspPartObject, bool> hasDependency)
		{
			var parts = craft.Children<KspPartObject> ().ToList ();
			var dependencies = new HashSet<KspPartObject> ();

			EvaluateDependencies (parts, dependency, hasDependency, dependencies.Add);

			return dependencies;
		}

		private static bool HasSoftDependency (KspPartObject part, KspPartObject dependency)
		{
			Func<IReadOnlyList<KspPartLinkProperty>, bool> hasSoftDependency =
				(refs) => refs.Any (r => Object.Equals(r.Part, dependency));

			return hasSoftDependency (part.LinkRefs) ||
			hasSoftDependency (part.ParentRefs) ||
			hasSoftDependency (part.SymRefs) ||
			hasSoftDependency (part.SrfNRefs) ||
			hasSoftDependency (part.AttNRefs);
		}

		private static bool HasHardDependency (KspPartObject part, KspPartObject dependency)
		{
			Func<IReadOnlyList<KspPartLinkProperty>, bool> hasHardDependency =
				(refs) => refs.Count > 0 && refs.All (r => Object.Equals(r.Part, dependency));
			
			return hasHardDependency (part.ParentRefs) ||
			hasHardDependency (part.SymRefs) ||
			hasHardDependency (part.SrfNRefs) ||
			hasHardDependency (part.AttNRefs);
		}

		private static void EvaluateDependencies (List<KspPartObject> parts, KspPartObject dependency, Func<KspPartObject, KspPartObject, bool> hasDependency, Func<KspPartObject, bool> addDependency) =>
			parts.ForEach (currentPart => {
				if (hasDependency (currentPart, dependency)) {
					if (addDependency (currentPart)) {
						EvaluateDependencies (parts, currentPart, hasDependency, addDependency);
					}
			}});
	}
}

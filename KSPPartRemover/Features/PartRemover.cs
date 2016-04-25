using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KSPPartRemover.Format;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Features
{
	public static class PartRemover
	{
		public static PartRemovalAction PrepareRemove (KspCraftObject craft, IReadOnlyList<KspPartObject> partsToRemove)
		{
			var partsToRemoveInclDependencies = new Dictionary<int, KspPartObject> ();

			foreach (var removedPart in partsToRemove) {
				CollectPartsToBeRemoved (craft, removedPart, partsToRemoveInclDependencies);
			}

			return new PartRemovalAction (craft, partsToRemoveInclDependencies);
		}

		private static void CollectPartsToBeRemoved (KspCraftObject craft, KspPartObject partToRemove, Dictionary<int, KspPartObject>  partsToRemoveInclDependencies = null)
		{
			var idOfpartToRemove = craft.GetIdOfPart (partToRemove);
			if (idOfpartToRemove < 0)
				return;
			
			partsToRemoveInclDependencies [idOfpartToRemove] = partToRemove;

			foreach (var part in craft.Parts.Value.Except(partsToRemoveInclDependencies.Values)) {
				var dependentOnIds = 
					GetPartReferencesFromProperty (part, "parent").Concat (
						GetPartReferencesFromProperty (part, "sym")).Concat (
						GetPartReferencesFromProperty (part, "srfN")).Concat (
						GetPartReferencesFromProperty (part, "attN")).
					Distinct ().Select (reference => IdOfPartReference (craft, reference));

				if (dependentOnIds.Any (dependendId => dependendId == idOfpartToRemove))
					CollectPartsToBeRemoved (craft, part, partsToRemoveInclDependencies);
			}
		}

		private static List<KspReference> GetPartReferencesFromProperty (KspPartObject part, string propertyName)
		{
			return part.kspObject.FindPropertyByName (propertyName).Select (property => KspObjectReader.ReadReference (property.value)).ToList ();
		}

		private static int IdOfPartReference (KspCraftObject craft, KspReference reference)
		{
			if (reference.id.HasValue) {
				return reference.id.Value;
			}

			var part = craft.Parts.Value.Where (p => p.Name.Value.Equals (reference.name)).SingleOrDefault ();

			return (part == null) ? -1 : craft.GetIdOfPart (part);
		}

		public class PartRemovalAction
		{
			public readonly KspCraftObject craft;
			public readonly Dictionary<int, KspPartObject> partsToBeRemoved;

			public PartRemovalAction (KspCraftObject craft, Dictionary<int, KspPartObject> partsToBeRemoved)
			{
				this.craft = craft;
				this.partsToBeRemoved = partsToBeRemoved;
			}

			public void RemoveParts ()
			{
				foreach (var part in craft.Parts.Value.Except(partsToBeRemoved.Values)) {
					if (!NeedsUpdate (part))
						continue;
					
					EvaluateAndUpdatePartReferences (part, "link");
					EvaluateAndUpdatePartReferences (part, "parent");
					EvaluateAndUpdatePartReferences (part, "sym");
					EvaluateAndUpdatePartReferences (part, "srfN");
					EvaluateAndUpdatePartReferences (part, "attN");
				}

				foreach (var entry in partsToBeRemoved) {
					craft.RemovePart (entry.Value);
				}
			}

			private void EvaluateAndUpdatePartReferences (KspPartObject part, string propertyName)
			{
				var updatedReferences = GetPartReferencesFromProperty (part, propertyName);
				if (updatedReferences.Any ()) {
					RemoveReferencesToRemovedParts (updatedReferences);
					AdjustReferenceIds (updatedReferences);
					UpdatePartReferences (part, propertyName, updatedReferences);
				}
			}

			private bool NeedsUpdate (KspPartObject part)
			{
				return
					GetPartReferencesFromProperty (part, "link").Concat (
					GetPartReferencesFromProperty (part, "parent")).Concat (
					GetPartReferencesFromProperty (part, "sym")).Concat (
					GetPartReferencesFromProperty (part, "srfN")).Concat (
					GetPartReferencesFromProperty (part, "attN")).
					Distinct ().
					Any (reference => partsToBeRemoved.Any (removeEntry => removeEntry.Key < IdOfPartReference (craft, reference)));
			}

			private void RemoveReferencesToRemovedParts (List<KspReference> references)
			{
				foreach (var part in partsToBeRemoved.OrderByDescending(entry => entry.Key)) {
					for (var referenceIdx = 0; referenceIdx < references.Count; referenceIdx++) {
						var reference = references [referenceIdx];
						var referencedId = IdOfPartReference (craft, reference);
						if (referencedId == part.Key)
							references.RemoveAt (referenceIdx--);
					}
				}
			}

			private void AdjustReferenceIds (List<KspReference> references)
			{
				foreach (var part in partsToBeRemoved.OrderByDescending(entry => entry.Key)) {
					for (var referenceIdx = 0; referenceIdx < references.Count; referenceIdx++) {
						var reference = references [referenceIdx];
						var referencedId = reference.id;
						if (referencedId.HasValue && referencedId.Value > part.Key)
							reference.name = (referencedId.Value - 1).ToString (CultureInfo.InvariantCulture);
					}
				}
			}

			private static void UpdatePartReferences (KspPartObject part, string propertyName, List<KspReference> references)
			{
				var existingProperties = part.kspObject.FindPropertyByName (propertyName).ToArray ();
				var insertindex = (existingProperties.Length > 0) ? part.kspObject.properties.IndexOf (existingProperties.First ()) : 0;

				foreach (var property in existingProperties) {
					part.kspObject.properties.Remove (property);
				}

				foreach (var reference in references) {
					part.kspObject.properties.Insert (insertindex++, new KspProperty (propertyName, KspObjectWriter.ToString (reference)));
				}
			}
		}
	}
}

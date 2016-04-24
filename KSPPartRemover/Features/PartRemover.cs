using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KSPPartRemover.Format;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Features
{
	public class PartRemover
	{
		private readonly KspObject craft;

		public PartRemover (KspObject craft)
		{
			this.craft = craft;
		}

		public PartRemovalAction PrepareRemovePart (KspObject partToRemove)
		{
			return new PartRemovalAction (this, CollectPartsToBeRemoved (partToRemove));
		}

		public PartRemovalAction CombineRemovalActions (IEnumerable<PartRemovalAction> partRemovalActions)
		{
			return new PartRemovalAction (this, partRemovalActions.SelectMany (action => action.PartsToBeRemoved).Distinct ().ToList ());
		}

		public class PartRemovalAction
		{
			public IReadOnlyList<KspObject> PartsToBeRemoved { get; private set; }

			public IReadOnlyList<int> PartIdsToBeRemovedDescendingOrder { get; private set; }

			private readonly PartRemover partRemover;

			public PartRemovalAction (PartRemover partRemover, IReadOnlyList<KspObject> partsToBeRemoved)
			{
				this.partRemover = partRemover;
				PartsToBeRemoved = partsToBeRemoved;
				PartIdsToBeRemovedDescendingOrder = PartsToBeRemoved.
					Select (part => partRemover.craft.GetIdOfPart (part)).
					Where (partId => partId >= 0).
					OrderByDescending (value => value).
					ToList ();
			}

			public void RemoveParts ()
			{
				foreach (var part in partRemover.craft.GetParts().Except(PartsToBeRemoved)) {
					if (!NeedsUpdate (part))
						continue;
					
					EvaluateAndUpdatePartReferences (part, "link");
					EvaluateAndUpdatePartReferences (part, "parent");
					EvaluateAndUpdatePartReferences (part, "sym");
					EvaluateAndUpdatePartReferences (part, "srfN");
					EvaluateAndUpdatePartReferences (part, "attN");
				}

				foreach (var partToRemove in PartsToBeRemoved) {
					partRemover.craft.children.Remove (partToRemove);
				}
			}

			private void EvaluateAndUpdatePartReferences (KspObject part, string propertyName)
			{
				var updatedReferences = GetPartReferencesFromProperty (part, propertyName);
				if (updatedReferences.Any ()) {
					RemoveReferencesToRemovedParts (updatedReferences);
					AdjustReferenceIds (updatedReferences);
					UpdatePartReferences (part, propertyName, updatedReferences);
				}
			}

			private bool NeedsUpdate (KspObject part)
			{
				return
					GetPartReferencesFromProperty (part, "link").Concat (
					GetPartReferencesFromProperty (part, "parent")).Concat (
					GetPartReferencesFromProperty (part, "sym")).Concat (
					GetPartReferencesFromProperty (part, "srfN")).Concat (
					GetPartReferencesFromProperty (part, "attN")).
					Distinct ().Any (
					reference => PartIdsToBeRemovedDescendingOrder.Any (
						idToBeRemoved => idToBeRemoved < partRemover.IdOfPartReference (reference)));
			}

			private void RemoveReferencesToRemovedParts (List<KspReference> references)
			{
				for (var idToBeRemovedIdx = 0; idToBeRemovedIdx < PartIdsToBeRemovedDescendingOrder.Count; idToBeRemovedIdx++) {
					for (var referenceIdx = 0; referenceIdx < references.Count; referenceIdx++) {
						var reference = references [referenceIdx];
						var referencedId = partRemover.IdOfPartReference (reference);
						if (referencedId == PartIdsToBeRemovedDescendingOrder [idToBeRemovedIdx])
							references.RemoveAt (referenceIdx--);
					}
				}
			}

			private void AdjustReferenceIds (List<KspReference> references)
			{
				for (var idToBeRemovedIdx = 0; idToBeRemovedIdx < PartIdsToBeRemovedDescendingOrder.Count; idToBeRemovedIdx++) {
					for (var referenceIdx = 0; referenceIdx < references.Count; referenceIdx++) {
						var reference = references [referenceIdx];
						if (reference.id > PartIdsToBeRemovedDescendingOrder [idToBeRemovedIdx])
							reference.name = (reference.id - 1).ToString (CultureInfo.InvariantCulture);
					}
				}
			}
		}

		private IReadOnlyList<KspObject> CollectPartsToBeRemoved (KspObject partToRemove, List<KspObject> partsToBeRemoved = null)
		{
			var idOfpartToRemove = craft.GetIdOfPart (partToRemove);
			if (idOfpartToRemove < 0)
				return new KspObject[0];

			if (partsToBeRemoved == null)
				partsToBeRemoved = new List<KspObject> ();

			partsToBeRemoved.Add (partToRemove);

			foreach (var part in craft.GetParts().Except(partsToBeRemoved)) {
				var dependentOnIds = 
					GetPartReferencesFromProperty (part, "parent").Concat (
						GetPartReferencesFromProperty (part, "sym")).Concat (
						GetPartReferencesFromProperty (part, "srfN")).Concat (
						GetPartReferencesFromProperty (part, "attN")).
					Distinct ().Select (IdOfPartReference);

				if (dependentOnIds.Any (dependendId => dependendId == idOfpartToRemove))
					CollectPartsToBeRemoved (part, partsToBeRemoved);
			}

			return partsToBeRemoved;
		}

		private static void UpdatePartReferences (KspObject part, string propertyName, List<KspReference> references)
		{
			var existingProperties = part.FindPropertyByName (propertyName).ToArray ();
			var insertindex = (existingProperties.Length > 0) ? part.properties.IndexOf (existingProperties.First ()) : 0;

			foreach (var property in existingProperties) {
				part.properties.Remove (property);
			}

			foreach (var reference in references) {
				part.properties.Insert (insertindex++, new KspProperty (propertyName, KspObjectWriter.ToString (reference)));
			}
		}

		private static List<KspReference> GetPartReferencesFromProperty (KspObject part, string propertyName)
		{
			return part.FindPropertyByName (propertyName).
				Select (property => KspObjectReader.ReadReference (property.value)).
				ToList ();
		}

		private int IdOfPartReference (KspReference reference)
		{
			if (reference.id >= 0) {
				return reference.id;
			}

			var part = craft.GetParts().Where (p => p.GetPartName().Equals(reference.name)).SingleOrDefault();

			return (part == null) ? -1 : craft.GetIdOfPart(part);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KSPPartRemover.Backend
{
	public class SafePartRemover
	{
		private readonly CraftFile CraftFile;

		public SafePartRemover(CraftFile craftFile)
		{
			CraftFile = craftFile;
		}

		public PartRemovalAction PrepareRemovePart(Part partToRemove)
		{
			return new PartRemovalAction(this, CollectPartsToBeRemoved(partToRemove));
		}

		public PartRemovalAction CombineRemovalActions(IEnumerable<PartRemovalAction> partRemovalActions)
		{
			return new PartRemovalAction(this, partRemovalActions.SelectMany(action => action.PartsToBeRemoved).Distinct().ToList());
		}

		public class PartRemovalAction
		{
			public IReadOnlyList<Part> PartsToBeRemoved { get; private set; }
			public IReadOnlyList<long> PartIdsToBeRemovedDescendingOrder { get; private set; }

			private readonly SafePartRemover SafePartRemover;

			public PartRemovalAction(SafePartRemover safePartRemover, IReadOnlyList<Part> partsToBeRemoved)
			{
				SafePartRemover = safePartRemover;
				PartsToBeRemoved = partsToBeRemoved;
				PartIdsToBeRemovedDescendingOrder = PartsToBeRemoved.
					Select(part => SafePartRemover.CraftFile.IdOfPart(part)).
					Where(partId => partId >= 0).
					OrderByDescending(value => value).
					ToList();
			}

			public void RemoveParts()
			{
				var partReplacementDictionary = new Dictionary<Part, Part>();
				foreach (var part in SafePartRemover.CraftFile.Except(PartsToBeRemoved))
				{
					if (!NeedsUpdate(part))
						continue;

					var updatedPart = Part.FromContent(part.Content);

					// remove links
					var updatedReferences = GetLinkPartReferences(updatedPart);
					if (updatedReferences.Any())
					{
						RemoveReferencesToRemovedParts(updatedReferences);
						SetLinkPartReferences(updatedPart, updatedReferences);
					}

					// parent
					updatedReferences = GetParentPartReferences(updatedPart);
					if (updatedReferences.Any())
					{
						UpdateReferences(updatedReferences);
						SetParentPartReferences(updatedPart, updatedReferences);
					}

					// sym
					updatedReferences = GetSymPartReferences(updatedPart);
					if (updatedReferences.Any())
					{
						UpdateReferences(updatedReferences);
						SetSymPartReferences(updatedPart, updatedReferences);
					}

					// SrfN
					updatedReferences = GetSrfNPartReferences(updatedPart);
					if (updatedReferences.Any())
					{
						UpdateReferences(updatedReferences);
						SetSrfNPartReferences(updatedPart, updatedReferences);
					}

					// AttN
					updatedReferences = GetAttNPartReferences(updatedPart);
					if (updatedReferences.Any())
					{
						UpdateReferences(updatedReferences);
						SetAttNPartReferences(updatedPart, updatedReferences);
					}

					partReplacementDictionary.Add(part, updatedPart);
				}

				foreach (var partToRemove in PartsToBeRemoved)
					SafePartRemover.CraftFile.RemovePart(partToRemove);

				foreach (var partReplacement in partReplacementDictionary)
					SafePartRemover.CraftFile.ReplacePart(partReplacement.Key, partReplacement.Value);
			}

			private bool NeedsUpdate(Part part)
			{
				if (GetLinkPartReferences(part).Any(
					partReference => PartIdsToBeRemovedDescendingOrder.Any(
						idToBeRemoved => idToBeRemoved == SafePartRemover.IdOfPartReference(partReference))))
					return true;

				return
					GetParentPartReferences(part).Concat(
						GetSymPartReferences(part)).Concat(
							GetSrfNPartReferences(part)).Concat(
								GetAttNPartReferences(part)).Any(
									partReference => PartIdsToBeRemovedDescendingOrder.Any(
										idToBeRemoved => idToBeRemoved < SafePartRemover.IdOfPartReference(partReference)));
			}

			private void RemoveReferencesToRemovedParts(IList<string> references)
			{
				for (var idToBeRemovedIdx = 0; idToBeRemovedIdx < PartIdsToBeRemovedDescendingOrder.Count; idToBeRemovedIdx++)
				{
					for (var referenceIdx = 0; referenceIdx < references.Count; referenceIdx++)
					{
						var referencedId = SafePartRemover.IdOfPartReference(references[referenceIdx]);
						if (referencedId == PartIdsToBeRemovedDescendingOrder[idToBeRemovedIdx])
							references.RemoveAt(referenceIdx--);
					}
				}
			}

			private void UpdateReferences(IList<string> references)
			{
				for (var idToBeRemovedIdx = 0; idToBeRemovedIdx < PartIdsToBeRemovedDescendingOrder.Count; idToBeRemovedIdx++)
				{
					for (var referenceIdx = 0; referenceIdx < references.Count; referenceIdx++)
					{
						long referencedId;
						if (!long.TryParse(references[referenceIdx], out referencedId))
							continue;

						if (referencedId > PartIdsToBeRemovedDescendingOrder[idToBeRemovedIdx])
							references[referenceIdx] = (referencedId - 1).ToString(CultureInfo.InvariantCulture);
					}
				}
			}
		}

		private IReadOnlyList<Part> CollectPartsToBeRemoved(Part partToRemove, List<Part> partsToBeRemoved = null)
		{
			var idOfpartToRemove = CraftFile.IdOfPart(partToRemove);
			if (idOfpartToRemove < 0)
				return new Part[0];

			if (partsToBeRemoved == null)
				partsToBeRemoved = new List<Part>();

			partsToBeRemoved.Add(partToRemove);

			foreach (var part in CraftFile.Except(partsToBeRemoved))
			{
				var dependentOnIds =
					GetParentPartReferences(part).Concat(
						GetSymPartReferences(part)).Concat(
							GetSrfNPartReferences(part)).Concat(
								GetAttNPartReferences(part)).Select(IdOfPartReference);

				if (dependentOnIds.Any(dependendId => dependendId == idOfpartToRemove))
					CollectPartsToBeRemoved(part, partsToBeRemoved);
			}

			return partsToBeRemoved;
		}

		private static IList<string> GetParentPartReferences(Part part)
		{
			return ParsePartReferenceOfProperty(part, "parent").ToList();
		}

		private static void SetParentPartReferences(Part part, IEnumerable<string> references)
		{
			part.SetMultiPropertyValues("parent", references.Select(id => id.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		private static IList<string> GetLinkPartReferences(Part part)
		{
			return ParsePartReferenceOfProperty(part, "link").ToList();
		}

		private static void SetLinkPartReferences(Part part, IEnumerable<string> references)
		{
			part.SetMultiPropertyValues("link", references.Select(id => id.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		private static IList<string> GetSymPartReferences(Part part)
		{
			return ParsePartReferenceOfProperty(part, "sym").ToList();
		}

		private static void SetSymPartReferences(Part part, IEnumerable<string> references)
		{
			part.SetMultiPropertyValues("sym", references.Select(id => id.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		private static IList<string> GetSrfNPartReferences(Part part)
		{
			return ParsePartReferenceOfProperty(part, "srfN").ToList();
		}

		private static void SetSrfNPartReferences(Part part, IEnumerable<string> references)
		{
			var srfNPrefixes = part.GetMultiPropertyValues("srfN").Select(entry => entry.Split(',')[0]).ToArray();
			var newSrfNEntries = new string[srfNPrefixes.Length];

			var idx = 0;
			foreach (var id in references)
				newSrfNEntries[idx] = string.Format("{0}, {1}", srfNPrefixes[idx++], id);

			part.SetMultiPropertyValues("srfN", newSrfNEntries);
		}

		private static IList<string> GetAttNPartReferences(Part part)
		{
			return ParsePartReferenceOfProperty(part, "attN").ToList();
		}

		private static void SetAttNPartReferences(Part part, IEnumerable<string> references)
		{
			var srfNPrefixes = part.GetMultiPropertyValues("attN").Select(entry => entry.Split(',')[0]).ToArray();
			var newSrfNEntries = new string[srfNPrefixes.Length];

			var idx = 0;
			foreach (var id in references)
				newSrfNEntries[idx] = string.Format("{0}, {1}", srfNPrefixes[idx++], id);

			part.SetMultiPropertyValues("attN", newSrfNEntries);
		}

		private static IEnumerable<string> ParsePartReferenceOfProperty(Part part, string propertyName)
		{
			return part.GetMultiPropertyValues(propertyName).
				Select(entry => entry.Contains(",") ? entry.Split(',')[1] : entry).
				Select(reference => reference.Trim());
		}

		private long IdOfPartReference(string partReference)
		{
			long id;
			if (!long.TryParse(partReference, out id))
				id = CraftFile.IdOfPart(partReference);

			return id;
		}
	}
}

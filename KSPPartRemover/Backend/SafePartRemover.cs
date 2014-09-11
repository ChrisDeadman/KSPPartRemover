using System;
using System.Collections.Generic;
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
					if (!GetAllReferencedIds(part).Any(id => PartIdsToBeRemovedDescendingOrder.Any(idToBeRemoved => idToBeRemoved < id)))
						continue;

					var updatedPart = Part.FromContent(part.Content);

					// parent
					var updatedIds = GetParentIds(updatedPart);
					if (updatedIds.Any())
					{
						UpdateReferencedIds(updatedIds);
						SetParentIds(updatedPart, updatedIds);
					}

					// sym
					updatedIds = GetSymIds(updatedPart);
					if (updatedIds.Any())
					{
						UpdateReferencedIds(updatedIds);
						SetSymIds(updatedPart, updatedIds);
					}

					// SrfN
					updatedIds = GetSrfNIds(updatedPart);
					if (updatedIds.Any())
					{
						UpdateReferencedIds(updatedIds);
						SetSrfNIds(updatedPart, updatedIds);
					}

					// AttN
					updatedIds = GetAttNIds(updatedPart);
					if (updatedIds.Any())
					{
						UpdateReferencedIds(updatedIds);
						SetAttNIds(updatedPart, updatedIds);
					}

					partReplacementDictionary.Add(part, updatedPart);
				}

				foreach (var partToRemove in PartsToBeRemoved)
					SafePartRemover.CraftFile.RemovePart(partToRemove);

				foreach (var partReplacement in partReplacementDictionary)
					SafePartRemover.CraftFile.ReplacePart(partReplacement.Key, partReplacement.Value);
			}

			private void UpdateReferencedIds(IList<long> referencedIds)
			{
				for (var referencedIdIdx = 0; referencedIdIdx < referencedIds.Count; referencedIdIdx++)
				{
					for (var idToBeRemovedIdx = 0; idToBeRemovedIdx < PartIdsToBeRemovedDescendingOrder.Count; idToBeRemovedIdx++)
					{
						if (referencedIds[referencedIdIdx] > PartIdsToBeRemovedDescendingOrder[idToBeRemovedIdx])
							referencedIds[referencedIdIdx] = referencedIds[referencedIdIdx] - 1;
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
				var referencedIds = GetAllReferencedIds(part);

				if (referencedIds.Any(id => id == idOfpartToRemove))
					CollectPartsToBeRemoved(part, partsToBeRemoved);
			}

			return partsToBeRemoved;
		}

		private static IEnumerable<long> GetAllReferencedIds(Part part)
		{
			return GetParentIds(part).Concat(GetSymIds(part)).Concat(GetSrfNIds(part)).Concat(GetAttNIds(part)).Distinct();
		}

		private static long[] GetParentIds(Part part)
		{
			return part.GetMultiPropertyValues("parent").Select(long.Parse).ToArray();
		}

		private static void SetParentIds(Part part, IEnumerable<long> ids)
		{
			part.SetMultiPropertyValues("parent", ids.Select(id => id.ToString()).ToArray());
		}

		private static long[] GetSymIds(Part part)
		{
			return part.GetMultiPropertyValues("sym").Select(long.Parse).ToArray();
		}

		private static void SetSymIds(Part part, IEnumerable<long> ids)
		{
			part.SetMultiPropertyValues("sym", ids.Select(id => id.ToString()).ToArray());
		}

		private static long[] GetSrfNIds(Part part)
		{
			return part.GetMultiPropertyValues("srfN").Select(srfn => long.Parse(srfn.Split(',')[1])).ToArray();
		}

		private static void SetSrfNIds(Part part, IEnumerable<long> ids)
		{
			var srfNPrefixes = part.GetMultiPropertyValues("srfN").Select(entry => entry.Split(',')[0]).ToArray();
			var newSrfNEntries = new string[srfNPrefixes.Length];

			var idx = 0;
			foreach (var id in ids)
				newSrfNEntries[idx] = string.Format("{0}, {1}", srfNPrefixes[idx++], id);

			part.SetMultiPropertyValues("srfN", newSrfNEntries);
		}

		private static long[] GetAttNIds(Part part)
		{
			return part.GetMultiPropertyValues("attN").Select(srfn => long.Parse(srfn.Split(',')[1])).ToArray();
		}

		private static void SetAttNIds(Part part, IEnumerable<long> ids)
		{
			var srfNPrefixes = part.GetMultiPropertyValues("attN").Select(entry => entry.Split(',')[0]).ToArray();
			var newSrfNEntries = new string[srfNPrefixes.Length];

			var idx = 0;
			foreach (var id in ids)
				newSrfNEntries[idx] = string.Format("{0}, {1}", srfNPrefixes[idx++], id);

			part.SetMultiPropertyValues("attN", newSrfNEntries);
		}
	}
}

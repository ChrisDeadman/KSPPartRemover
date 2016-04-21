using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPartRemover.Backend
{
	public class PartFinder
	{
		private readonly Craft Craft;

		public PartFinder(Craft craft)
		{
			Craft = craft;
		}

		public Part FindPartById(long id)
		{
			var currentPartId = 0;
			return Craft.FirstOrDefault(part => currentPartId++ == id);
		}

		public IReadOnlyList<Part> AllOccurrences(string name)
		{
			return Craft.Where(part => part.Name.Equals(name)).ToList();
		}
	}
}

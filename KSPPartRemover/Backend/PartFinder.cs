using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPartRemover.Backend
{
	public class PartFinder
	{
		private readonly CraftFile CraftFile;

		public PartFinder(CraftFile craftFile)
		{
			CraftFile = craftFile;
		}

		public Part FindPartById(long id)
		{
			var currentPartId = 0;
			return CraftFile.FirstOrDefault(part => currentPartId++ == id);
		}

		public IReadOnlyList<Part> AllOccurrences(string name)
		{
			return CraftFile.Where(part => part.Name.Equals(name)).ToList();
		}
	}
}

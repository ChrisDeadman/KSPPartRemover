using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPartRemover.Backend
{
	public class CraftFile : IEnumerable<Part>
	{
		public string Content { get; private set; }

		private CraftFile(string content)
		{
			Content = content;
		}

		public static CraftFile FromText(string content)
		{
			return new CraftFile(content);
		}

		public long IdOfPart(string partName)
		{
			var id = 0;
			foreach (var presentPart in this)
			{
				if (presentPart.Name.Equals(partName))
					return id;

				id++;
			}

			return -1;
		}

		public long IdOfPart(Part part)
		{
			var id = 0;
			foreach (var presentPart in this)
			{
				if (presentPart.Equals(part))
					return id;

				id++;
			}

			return -1;
		}

		public void ReplacePart(Part partToReplace, Part replacementPart)
		{
			Content = Content.Replace(partToReplace.Content, replacementPart.Content);
		}

		public void RemovePart(Part partToRemove)
		{
			Content = Content.Replace(partToRemove.Content, "").Replace("\n\n", Environment.NewLine).Replace("\r\n\r\n", Environment.NewLine);
		}

		public IEnumerator<Part> GetEnumerator()
		{
			var startIndex = 0;
			while (startIndex < Content.Length - 1)
			{
				var part = PartAtOrAfter(startIndex, out startIndex);
				if (part != null)
					yield return part;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		// NOTE: We do not use a regex for this because it's so ultimately slow that the app would not be usable anymore!
		private Part PartAtOrAfter(int startIndex, out int endIndex)
		{
			startIndex = Content.IndexOf("PART", startIndex);
			if (startIndex < 0)
			{
				endIndex = Content.Length;
				return null;
			}

			endIndex = Content.IndexOf("{", startIndex);

			var bracketLevel = 1;
			while (bracketLevel > 0)
			{
				if (++endIndex >= Content.Length)
					throw new FormatException();

				var nextOpeningBracketIdx = Content.IndexOf("{", endIndex);
				if (nextOpeningBracketIdx < 0)
					nextOpeningBracketIdx = int.MaxValue;
				var nextClosingBracketIdx = Content.IndexOf("}", endIndex);
				if (nextClosingBracketIdx < 0)
					nextClosingBracketIdx = int.MaxValue;

				if (nextOpeningBracketIdx < nextClosingBracketIdx)
				{
					bracketLevel++;
					endIndex = nextOpeningBracketIdx;
					continue;
				}

				if (nextClosingBracketIdx < nextOpeningBracketIdx)
				{
					bracketLevel--;
					endIndex = nextClosingBracketIdx;
					continue;
				}

				throw new FormatException();
			}

			return Part.FromContent(Content.Substring(startIndex, (endIndex - startIndex) + 1));
		}
	}
}

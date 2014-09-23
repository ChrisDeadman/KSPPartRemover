using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPartRemover.Backend
{
	public class Part
	{
		public string Content { get; private set; }

		public Part(string name, params KeyValuePair<string, string>[] properties)
		{
			var content = new StringBuilder();
			content.AppendLine("PART ");
			content.AppendLine("{");
			content.AppendFormat("name = {0}", name);
			content.AppendLine();
			foreach (var property in properties)
			{
				content.AppendFormat("{0} = {1}", property.Key, property.Value);
				content.AppendLine();
			}
			content.Append("}");

			Content = content.ToString();
		}

		private Part(string content)
		{
			Content = content;
		}

		public static Part FromContent(string content)
		{
			return new Part(content);
		}

		public string Name
		{
			get
			{
				int endIdx;

				var contentIndex = 0;
				var value = FindProperty("part", ref contentIndex, out endIdx);
				if (value != null)
					return value;

				contentIndex = 0;
				value = FindProperty("name", ref contentIndex, out endIdx);
				if (value != null)
					return value;

				throw new FormatException();
			}
		}

		public IList<string> GetMultiPropertyValues(string key)
		{
			var values = new List<string>();

			var contentIndex = 0;
			while (true)
			{
				int endIdx;
				var value = FindProperty(key, ref contentIndex, out endIdx);
				if (value == null)
					break;

				values.Add(value);
				contentIndex = endIdx;
			}

			return values;
		}

		public void SetMultiPropertyValues(string key, params string[] values)
		{
			var contentIndex = 0;

			// we do not support setting non-existing properties at the moment because we don't know where they should possibly belong
			int endIdx;
			if (FindProperty(key, ref contentIndex, out endIdx) == null)
				throw new NotSupportedException();

			var preamble = PrecedingWhitespaceCharacters(contentIndex);
			contentIndex -= preamble.Length;

			RemoveMultiPropertyValues(key);

			foreach (var value in values)
			{
				var propertyString = preamble + string.Format("{0} = {1}", key, value) + Environment.NewLine;

				Content = Content.Insert(contentIndex, propertyString);
				contentIndex += propertyString.Length;
			}
		}

		public void RemoveMultiPropertyValues(string key)
		{
			var contentIndex = 0;
			while (true)
			{
				int endIdx;
				var value = FindProperty(key, ref contentIndex, out endIdx);
				if (value == null)
					break;

				var preamble = PrecedingWhitespaceCharacters(contentIndex);
				contentIndex -= preamble.Length;

				Content = Content.Remove(contentIndex, (endIdx - contentIndex) + 1);
			}
		}

		// NOTE: We do not use a regex for this because it's so ultimately slow that the app would not be usable anymore!
		private string FindProperty(string key, ref int startIndex, out int endIdx)
		{
			while (true)
			{
				endIdx = startIndex;

				var foundStartIdx = Content.IndexOf(key, startIndex);
				if (foundStartIdx < 1)
					return null;

				startIndex = foundStartIdx;

				var charBeforeStartidx = Content[startIndex - 1];
				if (charBeforeStartidx != ' ' && charBeforeStartidx != '\t' && charBeforeStartidx != '{' && charBeforeStartidx != '\n')
				{
					startIndex++;
					continue;
				}

				endIdx = Content.IndexOf('\n', startIndex + 1);
				if (endIdx < 0)
					endIdx = Content.Length - 1;

				var potentialProperty = Content.Substring(startIndex, endIdx - startIndex);
				if (!potentialProperty.Contains("="))
				{
					startIndex = endIdx;
					continue;
				}

				return potentialProperty.Split('=')[1].Trim();
			}
		}

		private string PrecedingWhitespaceCharacters(int index)
		{
			var previousNewLineIdx = Content.LastIndexOfAny(new[] {'\r', '\n'}, index);
			previousNewLineIdx++; // This also covers the -1 if not found

			return Content.Substring(previousNewLineIdx, index - previousNewLineIdx);
		}

		public override string ToString()
		{
			return Content;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Part);
		}

		protected bool Equals(Part other)
		{
			if (other == null)
				return false;

			return Equals(Content, other.Content);
		}

		public override int GetHashCode()
		{
			return (Content != null ? Content.GetHashCode() : 0);
		}
	}
}

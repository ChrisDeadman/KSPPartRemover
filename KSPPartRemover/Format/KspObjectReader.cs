using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Format
{
	public class KspObjectReader
	{
		public static KspObject Read (String text)
		{
			var lines = text.Split (new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select (str => str.TrimStart ()).ToArray ();

			KspObject rootObject;
			ReadObject (lines, 0, out rootObject);
			return rootObject;
		}

		private static int ReadObject (string[] lines, int index, out KspObject obj)
		{
			if (lines.Length <= (index + 1)) {
				throw new FormatException ();
			}

			var name = KspCraftFileExtensions.GlobalVesselType;
			if (!lines [index].Contains ("=")) {
				name = lines [index++].Trim();
				if (lines [index++] != "{") {
					throw new FormatException ();
				}
			}

			List<KspProperty> properties;
			List<KspObject> objects;

			index = ReadProperties (lines, index, out properties);
			index = ReadObjects (lines, index, out objects);

			if ((lines.Length > index) && lines [index] != "}") {
				throw new FormatException ();
			}

			obj = new KspObject (name, properties, objects);

			return index + 1;
		}

		static int ReadProperties (string[] lines, int index, out List<KspProperty> properties)
		{
			properties = new List<KspProperty> ();

			while (lines.Length > index) {
				var keyValue = lines [index].Split ('=');
				if (keyValue.Length != 2) {
					break;
				}

				properties.Add (new KspProperty (keyValue [0].Trim(), keyValue [1].Trim()));
				index++;
			}

			return index;
		}

		static int ReadObjects (string[] lines, int index, out List<KspObject> objects)
		{
			objects = new List<KspObject> ();

			while (lines.Length > index) {
				KspObject obj;

				if (lines [index] == "}") {
					break;
				}

				var nextIndex = ReadObject (lines, index, out obj);
				if (nextIndex <= index) {
					break;
				}
				index = nextIndex;

				objects.Add (obj);
			}

			return index;
		}
	}
}

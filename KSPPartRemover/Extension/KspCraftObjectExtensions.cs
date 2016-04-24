using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Format;
using System.Text.RegularExpressions;

namespace KSPPartRemover.Extension
{
	public static class KspCraftObjectExtensions
	{
		public static IEnumerable<KspObject> GetCrafts (this KspObject obj)
		{
			if (obj.IsGlobalVessel ()) {
				return new[] { obj };
			}

			return obj.FindChildByType ("VESSEL", recursive: true);
		}

		public static IEnumerable<KspObject> GetParts (this KspObject obj)
		{
			return obj.FindChildByType ("PART");
		}

		public static IEnumerable<KspObject> FilterCraftsByNamePattern (this KspObject obj, String namePattern)
		{
			return obj.GetCrafts ().Where (craft => MatchesRegex (craft.GetCraftName (), namePattern));
		}

		public static IEnumerable<KspObject> FilterPartsByNamePattern (this KspObject obj, String namePattern)
		{
			return obj.GetParts ().Where (part => MatchesRegex (part.GetPartName (), namePattern));
		}

		public static String GetCraftName (this KspObject obj)
		{
			var property = obj.FindPropertyByName ("ship").FirstOrDefault () ?? obj.FindPropertyByName ("name").FirstOrDefault ();
			return property.value;
		}

		public static String GetPartName (this KspObject obj)
		{
			var property = obj.FindPropertyByName ("part").FirstOrDefault () ?? obj.FindPropertyByName ("name").FirstOrDefault ();
			return property.value;
		}

		public static KspObject GetPartById (this KspObject obj, int id)
		{
			var parts = obj.GetParts ().ToList ();
			return (parts.Count > id) ? parts [id] : null;
		}

		public static int GetIdOfPart (this KspObject obj, KspObject part)
		{
			var parts = obj.GetParts ().ToList ();
			return parts.IndexOf (part);
		}

		private static bool MatchesRegex (String str, String pattern)
		{
			return String.IsNullOrEmpty (pattern) || Regex.Match (str, pattern).Success;
		}
	}
}

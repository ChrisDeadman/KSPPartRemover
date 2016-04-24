using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Format;

namespace KSPPartRemover.Extension
{
	public static class KspObjectSearchExtensions
	{
		public static IEnumerable<KspProperty> FindPropertyByName (this KspObject obj, String name)
		{
			return obj.properties.Where (property => property.name.Equals (name));
		}

		public static IEnumerable<KspObject> FindChildByType (this KspObject obj, String type, bool recursive = false)
		{
			foreach (var child in obj.children) {
				if (child.type.Equals (type)) {
					yield return child;
				}

				if (recursive) {
					var childResults = child.FindChildByType (type, recursive);
					foreach (var match in childResults) {
						yield return match;
					}
				}
			}
		}
	}
}

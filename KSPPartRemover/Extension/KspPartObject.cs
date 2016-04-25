using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.Format;

namespace KSPPartRemover.Extension
{
	public class KspPartObject
	{
		public const String Type = "PART";

		public readonly KspObject kspObject;
		public readonly Lazy<String> Name;

		private KspPartObject (KspObject objectBehind)
		{
			this.kspObject = objectBehind;
			this.Name = new Lazy<String> (GetName);
		}

		public static KspPartObject From (KspObject objectBehind)
		{
			return new KspPartObject (objectBehind);
		}

		private String GetName ()
		{
			var property = kspObject.FindPropertyByName ("part").FirstOrDefault () ?? kspObject.FindPropertyByName ("name").FirstOrDefault ();
			return property.value;
		}

		public override bool Equals (object other)
		{
			return Equals(other as KspPartObject);
		}

		public bool Equals (KspPartObject other)
		{
			return other != null && kspObject == other.kspObject;
		}

		public override int GetHashCode ()
		{
			unchecked {
				return (kspObject != null ? kspObject.GetHashCode () : 0);
			}
		}
	}
}

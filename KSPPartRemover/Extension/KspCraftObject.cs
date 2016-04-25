using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.Format;

namespace KSPPartRemover.Extension
{
	public class KspCraftObject
	{
		public const String Type = "VESSEL";

		public readonly KspObject kspObject;
		public readonly Lazy<String> Name;
		public readonly Lazy<List<KspPartObject>> Parts;

		private KspCraftObject (KspObject kspObject)
		{
			this.kspObject = kspObject;
			this.Name = new Lazy<String> (GetName);
			this.Parts = new Lazy<List<KspPartObject>> (GetParts);
		}

		public static KspCraftObject From (KspObject objectBehind)
		{
			return new KspCraftObject (objectBehind);
		}

		private String GetName ()
		{
			var property = kspObject.FindPropertyByName ("ship").FirstOrDefault () ?? kspObject.FindPropertyByName ("name").FirstOrDefault ();
			return property.value;
		}

		private List<KspPartObject> GetParts ()
		{
			return kspObject.FindChildByType (KspPartObject.Type).Select(KspPartObject.From).ToList ();
		}

		public KspPartObject GetPartById (int id)
		{
			return (Parts.Value.Count > id) ? Parts.Value [id] : null;
		}

		public int GetIdOfPart (KspPartObject part)
		{
			return Parts.Value.IndexOf (part);
		}

		public void RemovePart (KspPartObject part)
		{
			kspObject.children.Remove (part.kspObject);
		}

		public override bool Equals (object other)
		{
			return Equals(other as KspCraftObject);
		}

		public bool Equals (KspCraftObject other)
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

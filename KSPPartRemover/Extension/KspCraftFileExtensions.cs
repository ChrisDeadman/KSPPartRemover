using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.Format;

namespace KSPPartRemover.Extension
{
	public static class KspCraftFileExtensions
	{
		public const String GlobalVesselType = "KSP_PART_REMOVER_GLOBAL_VESSEL";

		public static bool IsGlobalVessel (this KspObject obj)
		{
			return obj.type.Equals (GlobalVesselType);
		}

		public static IList<KspCraftObject> GetCrafts (this KspObject obj)
		{
			if (obj.IsGlobalVessel ()) {
				return new[] { KspCraftObject.From(obj) };
			}

			return obj.FindChildByType (KspCraftObject.Type, recursive: true).Select(KspCraftObject.From).ToList ();
		}
	}
}

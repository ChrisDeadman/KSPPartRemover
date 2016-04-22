using System;
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
	}
}

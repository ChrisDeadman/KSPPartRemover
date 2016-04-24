using System;
using KSPPartRemover.Format;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Tests.TestHelpers
{
	public static class KspObjectGenerator
	{
		public static KspProperty Property (string name, string value)
		{
			return new KspProperty (name, value);
		}

		public static List<KspProperty> Properties(params KspProperty[] properties)
		{
			return properties.ToList ();
		}

		public static KspObject Object (string type, List<KspProperty> properties, params KspObject[] parts)
		{
			return new KspObject (type, properties.ToList(), parts.ToList());
		}

		public static KspObject Part (List<KspProperty> properties)
		{
			return Object ("PART", properties);
		}

		public static KspObject Craft (List<KspProperty> properties, params KspObject[] parts)
		{
			return Object ("VESSEL", properties, parts);
		}

		public static KspObject GlobalCraft (params KspObject[] parts)
		{
			return Object (KspCraftFileExtensions.GlobalVesselType, Properties(Property("dummy", "at least one property is needed for global object detection to work")), parts);
		}
	}
}

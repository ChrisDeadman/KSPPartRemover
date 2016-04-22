using System;
using System.Collections.Generic;
using System.Linq;

namespace KSPPartRemover.Format
{
	public class KspObject
	{
		public readonly string type;
		public readonly List<KspProperty> properties;
		public readonly List<KspObject> children;

		public KspObject (string type, List<KspProperty> properties, List<KspObject> children)
		{
			this.type = type;
			this.properties = properties;
			this.children = children;
		}
	}
}

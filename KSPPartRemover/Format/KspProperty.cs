using System;
using System.Collections.Generic;

namespace KSPPartRemover.Format
{
	public class KspProperty
	{
		public readonly string name;
		public readonly String value;

		public KspProperty (string name, String value)
		{
			this.name = name;
			this.value = value;
		}
	}
}

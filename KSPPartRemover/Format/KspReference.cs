using System;

namespace KSPPartRemover.Format
{
	public class KspReference
	{
		public readonly String prefix;

		public String name { get; set; }

		public int id {
			get { 
				int id;
				if (!int.TryParse (name, out id))
					id = -1;

				return id;
			}
		}

		public KspReference (String prefix, String reference)
		{
			this.prefix = prefix;
			this.name = reference;
		}
	}
}

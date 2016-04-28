using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.Extension;

namespace KSPPartRemover.KspObjects
{
    public class KspCraftObject : KspObject
    {
        public const String TypeId = "VESSEL";

        public KspCraftObject (bool isGlobalObject = false) : base (TypeId, isGlobalObject)
        {
        }

        public String Name => (this.Properties<KspStringProperty> ("ship").FirstOrDefault () ?? this.Properties<KspStringProperty> ("name").FirstOrDefault ()).Text;
    }
}

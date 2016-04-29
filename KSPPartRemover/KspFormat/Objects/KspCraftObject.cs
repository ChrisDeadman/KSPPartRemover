using System;
using System.Linq;

namespace KSPPartRemover.KspFormat.Objects
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

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

        public String Name => this.FirstProperty<KspStringProperty>("ship", "name").Text.Split("//")[0].Trim();
    }
}

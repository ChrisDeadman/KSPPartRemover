using System;

namespace KSPPartRemover.KspObjects
{
    public class KspPartLinkProperty : KspProperty
    {
        public String Prefix { get; }

        public KspPartObject Part { get; }

        public bool IsIdReference { get; }

        public KspPartLinkProperty (String name, String prefix, KspPartObject part, bool isIdReference = false) : base (name)
        {
            this.Prefix = prefix;
            this.Part = part;
            this.IsIdReference = isIdReference;
        }
    }
}

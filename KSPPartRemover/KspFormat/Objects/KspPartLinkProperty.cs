using System;

namespace KSPPartRemover.KspFormat.Objects
{
    public class KspPartLinkProperty : KspProperty
    {
        public static class Types
        {
            public static readonly String Link = "link";
            public static readonly String Parent = "parent";
            public static readonly String Sym = "sym";
            public static readonly String SrfN = "srfN";
            public static readonly String AttN = "attN";
        }

        public String Prefix { get; }

        public KspPartObject Part { get; }

        public string Postfix { get; }

        public bool IsIdReference { get; }

        public KspPartLinkProperty (String name,
                                    String prefix,
                                    KspPartObject part,
                                    string postfix = null,
                                    bool isIdReference = false) : base (name)
        {
            this.Prefix = prefix;
            this.Part = part;
            this.Postfix = postfix;
            this.IsIdReference = isIdReference;
        }
    }
}

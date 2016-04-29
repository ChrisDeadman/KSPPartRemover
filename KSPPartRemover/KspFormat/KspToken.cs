using System;
using System.Collections.Generic;

namespace KSPPartRemover.KspFormat
{
    public class KspToken
    {
        public String Name { get; }

        public IReadOnlyList<KeyValuePair<String, String>> Attributes { get; }

        public IReadOnlyList<KspToken> Tokens { get; }

        public KspToken (String Name, IReadOnlyList<KeyValuePair<String, String>> attributes, IReadOnlyList<KspToken> tokens)
        {
            this.Name = Name;
            this.Attributes = attributes;
            this.Tokens = tokens;
        }
    }
}

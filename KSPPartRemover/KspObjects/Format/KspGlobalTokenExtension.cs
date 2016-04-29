using System;
using System.Collections.Generic;

namespace KSPPartRemover.KspObjects.Format
{
    public static class KspGlobalTokenExtension
    {
        private const String GlobalTokenName = "KSPPR_GLOBAL_TOKEN";

        public static KspToken CreateGlobalToken (IReadOnlyList<KeyValuePair<String, String>> attributes, IReadOnlyList<KspToken> tokens)
        {
            return new KspToken (GlobalTokenName, attributes, tokens);
        }

        public static bool IsGlobalToken (this KspToken token)
        {
            return Object.Equals (token.Name, GlobalTokenName);
        }
    }
}

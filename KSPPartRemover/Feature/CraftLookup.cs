using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Feature
{
    public class CraftLookup
    {
        private readonly KspObject kspObjTree;

        public CraftLookup (KspObject kspObjTree)
        {
            this.kspObjTree = kspObjTree;
        }

        public IEnumerable<KspCraftObject> LookupCrafts (RegexFilter craftFilter)
        {
            var allCrafts = (kspObjTree is KspCraftObject) ? new[] { kspObjTree as KspCraftObject } : kspObjTree.Children <KspCraftObject> (recursive: true);

            return craftFilter.Apply (allCrafts, craft => craft.Name);
        }
    }
}

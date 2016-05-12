using System;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.KspFormat;

namespace KSPPartRemover.Feature
{
    public class CraftLoader
    {
        public static IReadOnlyList<KspCraftObject> Load (String craftFileText)
        {
            KspObject kspObjTree;
            return Load (craftFileText, out kspObjTree);
        }

        public static IReadOnlyList<KspCraftObject> Load (String craftFileText, out KspObject kspObjTree)
        {
            kspObjTree = KspObjectReader.ReadObject (KspTokenReader.ReadToken (craftFileText));

            var allCrafts = new List<KspCraftObject> ();

            if (kspObjTree is KspCraftObject) {
                allCrafts.Add (kspObjTree as KspCraftObject);
            } else {
                allCrafts.AddRange (kspObjTree.Children <KspCraftObject> (recursive: true));
            }

            return allCrafts;
        }
    }
}

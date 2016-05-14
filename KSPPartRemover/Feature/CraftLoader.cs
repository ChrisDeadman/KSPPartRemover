using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.KspFormat;

namespace KSPPartRemover.Feature
{
    public class CraftLoader
    {
        public static IReadOnlyList<KspCraftObject> LoadFromFile (String filePath)
        {
            KspObject kspObjTree;
            return LoadFromFile (filePath, out kspObjTree);
        }

        public static IReadOnlyList<KspCraftObject> LoadFromFile (String filePath, out KspObject kspObjTree)
        {
            using (var textReader = new StreamReader (new FileStream (filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8)) {
                return LoadFromText (textReader.ReadToEnd (), out kspObjTree);
            }
        }

        public static IReadOnlyList<KspCraftObject> LoadFromText (String craftFileText, out KspObject kspObjTree)
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

        public static void SaveToFile (String filePath, KspObject kspObjTree)
        {
            var craftToken = KspObjectWriter.WriteObject (kspObjTree);
            var craftString = KspTokenWriter.WriteToken (craftToken, new StringBuilder ()).ToString ();

            using (var textWriter = new StreamWriter (File.Create (filePath), Encoding.UTF8)) {
                textWriter.Write (craftString);
            }
        }
    }
}

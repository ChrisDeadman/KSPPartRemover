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
        public static KspObject LoadFromFile (String filePath)
        {
            using (var textReader = new StreamReader (new FileStream (filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8)) {
                return LoadFromText (textReader.ReadToEnd ());
            }
        }

        public static KspObject LoadFromText (String craftFileText)
        {
            return KspObjectReader.ReadObject (KspTokenReader.ReadToken (craftFileText));
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

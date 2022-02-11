using System;
using System.Text;
using System.IO;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.KspFormat;

namespace KSPPartRemover.Feature
{
    public class ObjectLoader
    {
        public static KspObject LoadFromFile(String filePath)
        {
            using (var textReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8)) {
                return LoadFromText(textReader.ReadToEnd());
            }
        }

        public static KspObject LoadFromText(String text)
        {
            return KspObjectReader.ReadObject(KspTokenReader.ReadToken(text));
        }

        public static void SaveToFile(String filePath, KspObject kspObjTree)
        {
            var kspToken = KspObjectWriter.WriteObject(kspObjTree);
            var kspTokenText = KspTokenWriter.WriteToken(kspToken, new StringBuilder()).ToString();

            using (var textWriter = new StreamWriter(File.Create(filePath), Encoding.UTF8)) {
                textWriter.Write(kspTokenText);
            }
        }
    }
}

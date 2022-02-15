using System;
using System.Collections.Generic;
using System.IO;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Feature
{
    public class PartDatabase
    {
        private Dictionary<string, string> Db;

        public PartDatabase(Dictionary<string, string> db)
        {
            this.Db = db;
        }

        public PartInfo GetPartInfo(string partName)
        {
            var modName = Db.GetValueOrDefault(partName);
            // if not found try with underlines in part name
            if (modName == null) {
                partName = partName.Replace('.', '_');
                modName = Db.GetValueOrDefault(partName);
            }
            return new PartInfo(partName, modName);
        }

        public static PartDatabase CreateFromDirectory(String path, Action<string> callback = null)
        {
            var db = new Dictionary<string, string>();

            foreach (var dir in Directory.EnumerateDirectories(path)) {
                callback?.Invoke(dir);

                var modName = Path.GetFileName(dir);

                foreach (var partsDir in Directory.GetDirectories(dir, "Parts", SearchOption.AllDirectories)) {
                    foreach (var partFile in Directory.GetFiles(partsDir, "*.cfg", SearchOption.AllDirectories)) {
                        var part = ObjectLoader.LoadFromFile(partFile) as KspPartObject;
                        if (part != null) {
                            db[part.Name] = modName;
                        }
                    }
                }
            }

            return new PartDatabase(db);
        }
    }
}

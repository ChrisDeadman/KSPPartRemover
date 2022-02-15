using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KSPPartRemover.Feature;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Command
{
    public class ListMods
    {
        private static readonly string ModFolder = "GameData";

        private readonly ProgramUI ui;

        public ListMods(ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute(String inputFilePath, RegexFilter craftFilter)
        {
            if (!Directory.Exists(ModFolder)) {
                ui.DisplayErrorMessage($"{ModFolder} directory not found! (is KSP directory your current directory?)");
                return -1;
            }

            ui.DisplayUserMessage("building part database...");
            var partDb = PartDatabase.CreateFromDirectory(ModFolder, dir => ui.DisplayUserMessage($"  {dir}..."));
            ui.DisplayUserMessage("done.\n");

            ui.DisplayUserMessage($"Searching for crafts matching '{craftFilter}'...");

            var kspObjTree = ObjectLoader.LoadFromFile(inputFilePath);
            var crafts = new CraftLookup(kspObjTree).LookupCrafts(craftFilter).ToList();

            var partInfos = crafts.ToDictionary(craft => craft, craft => FindPartInfos(partDb, craft));

            if (partInfos.Any(entry => entry.Value.Count > 0)) {
                ui.DisplayPartInfoList(partInfos);
            }

            return 0;
        }

        private List<PartInfo> FindPartInfos(PartDatabase partDb, KspCraftObject craft)
        {
            ui.DisplayUserMessage($"Entering craft '{craft.Name}'...");

            var filter = new RegexFilter("");
            var partLookup = new PartLookup(craft);
            var parts = partLookup.LookupParts(filter)
                .Select(p => p.Name.Split('_')[0])
                .Distinct()
                .Select(name => partDb.GetPartInfo(name))
                .Where(info => info.ModName != "Squad")
                .ToList();

            ui.DisplayUserMessage($"Found {parts.Count} mod parts");

            return parts;
        }
    }
}

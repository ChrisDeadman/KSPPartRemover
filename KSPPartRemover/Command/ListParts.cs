using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Command
{
    public class ListParts
    {
        private readonly ProgramUI ui;

        public ListParts(ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute(String inputFilePath, RegexFilter craftFilter, RegexFilter partFilter)
        {
            ui.DisplayUserMessage($"Searching for crafts matching '{craftFilter}'...");

            var kspObjTree = ObjectLoader.LoadFromFile(inputFilePath);
            var crafts = new CraftLookup(kspObjTree).LookupCrafts(craftFilter);

            ui.DisplayUserMessage($"Searching for parts matching '{partFilter}'...");

            var filteredParts = crafts.ToDictionary(craft => craft, craft => FindParts(craft, partFilter));

            if (filteredParts.Any(entry => entry.Value.Count > 0)) {
                ui.DisplayPartList(filteredParts);
            }

            return 0;
        }

        private List<KspPartObject> FindParts(KspCraftObject craft, RegexFilter filter)
        {
            ui.DisplayUserMessage($"Entering craft '{craft.Name}'...");

            var partLookup = new PartLookup(craft);
            var parts = partLookup.LookupParts(filter).ToList();

            ui.DisplayUserMessage($"Found {parts.Count} matching parts");

            return parts;
        }
    }
}

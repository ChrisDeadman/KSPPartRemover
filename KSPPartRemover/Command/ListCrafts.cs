using System;
using System.Linq;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Command
{
    public class ListCrafts
    {
        private readonly ProgramUI ui;

        public ListCrafts(ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute(String inputFilePath, RegexFilter craftFilter)
        {
            ui.DisplayUserMessage($"Searching for crafts matching '{craftFilter}'...");

            var kspObjTree = CraftLoader.LoadFromFile(inputFilePath);
            var crafts = new CraftLookup(kspObjTree).LookupCrafts(craftFilter).ToList();

            if (crafts.Count > 0) {
                ui.DisplayCraftList(crafts);
            }

            return 0;
        }
    }
}

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Command
{
    public class ListParts
    {
        private readonly ProgramUI ui;

        public ListParts (ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute (Parameters parameters)
        {
            var allCrafts = CraftLoader.Load (parameters.InputText);
            ui.DisplayUserMessage ($"Searching for crafts matching '{parameters.CraftFilter}'...");
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name);
            ui.DisplayUserMessage ($"Searching for parts matching '{parameters.PartFilter}'...");
            var filteredParts = filteredCrafts.ToDictionary (craft => craft, craft => FindParts (craft, parameters.PartFilter));

            ui.DisplayPartList (filteredParts);

            return 0;
        }

        private List<KspPartObject> FindParts (KspCraftObject craft, RegexFilter filter)
        {
            ui.DisplayUserMessage ($"Entering craft '{craft.Name}'...");
            var partLookup = new PartLookup (craft);
            var parts = partLookup.LookupParts (filter).ToList ();
            ui.DisplayUserMessage ($"Found {parts.Count} matching parts");
            return parts;
        }
    }
}

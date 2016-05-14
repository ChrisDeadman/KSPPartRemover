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
    public class ListCrafts
    {
        private readonly ProgramUI ui;

        public ListCrafts (ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute (Parameters parameters)
        {
            var allCrafts = CraftLoader.Load (parameters.InputText);
            ui.DisplayUserMessage ($"Searching for crafts matching '{parameters.CraftFilter}'...");
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name);

            ui.DisplayCraftList (filteredCrafts);

            return 0;
        }
    }
}

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;
using System.Threading.Tasks;

namespace KSPPartRemover.Command
{
    public class RemoveParts
    {
        private readonly ProgramUI ui;

        public RemoveParts (ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute (Parameters parameters)
        {
            KspObject kspObjTree;
            var allCrafts = CraftLoader.LoadFromFile (parameters.InputFilePath, out kspObjTree);
            ui.DisplayUserMessage ($"Searching for crafts matching '{parameters.CraftFilter}'...");
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name).ToList ();

            if (filteredCrafts.Count <= 0) {
                Console.WriteLine ($"No craft matching '{parameters.CraftFilter}' found, aborting");
                return -1;
            }

            ui.DisplayUserMessage ($"Searching for parts matching '{parameters.PartFilter}'...");

            var partsRemoved = false;

            foreach (var craft in filteredCrafts) {
                var toBeRemoved = FindRemovedAndDependentParts (craft, parameters.PartFilter);
                if (toBeRemoved.Count <= 0) {
                    continue;
                }

                ui.DisplayUserList ("Removed Parts", toBeRemoved.Select (part => ProgramUI.PartObjectToString (craft, part)));

                var removeParts = ui.AskYesNoQuestion ("Remove the listed parts?");
                if (removeParts) {
                    craft.Edit ().RemoveParts (toBeRemoved);
                    ui.DisplayUserMessage ($"{toBeRemoved.Count} parts removed");
                }

                partsRemoved |= removeParts;
            }

            if (!partsRemoved) {
                ui.DisplayErrorMessage ($"No parts removed");
                return -1;
            }

            CraftLoader.SaveToFile (parameters.OutputFilePath, kspObjTree);

            return 0;
        }

        private List<KspPartObject> FindRemovedAndDependentParts (KspCraftObject craft, RegexFilter filter)
        {
            ui.DisplayUserMessage ($"Entering craft '{craft.Name}'...");
            var partLookup = new PartLookup (craft);
            var removedParts = partLookup.LookupParts (filter).ToList ();
            ui.DisplayUserMessage ($"Found {removedParts.Count} parts to be removed");

            var dependentParts = new HashSet<KspPartObject> ();
            Parallel.ForEach (removedParts, removedPart => {
                foreach (var part in partLookup.LookupHardDependencies (removedPart)) {
                    lock (dependentParts) {
                        dependentParts.Add (part);
                    }
                }
            });
            ui.DisplayUserMessage ($"Found {dependentParts.Count} dependent parts");

            return removedParts.Concat (dependentParts).ToList ();
        }
    }
}

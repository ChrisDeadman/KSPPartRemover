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
                ui.DisplayErrorMessage ($"No craft matching '{parameters.CraftFilter}' found, aborting");
                return -1;
            }

            ui.DisplayUserMessage ($"Searching for parts matching '{parameters.PartFilter}'...");

            var partsRemoved = filteredCrafts.Aggregate (false, (removed, craft) => removed | RemoveMatchingParts (craft.Edit (), parameters.PartFilter));

            if (!partsRemoved) {
                ui.DisplayErrorMessage ($"No parts removed");
                return -1;
            }

            CraftLoader.SaveToFile (parameters.OutputFilePath, kspObjTree);

            return 0;
        }

        private bool RemoveMatchingParts (CraftEditor craftEditor, RegexFilter partFilter)
        {
            var toBeRemoved = FindRemovedAndDependentParts (craftEditor.Craft, partFilter);
            if (toBeRemoved.Count <= 0) {
                return false;
            }

            ui.DisplayUserList ("Removed Parts", toBeRemoved.Select (part => ProgramUI.PartObjectToString (craftEditor.Craft, part)));

            var removeConfirmed = ui.AskYesNoQuestion ("Remove the listed parts?");

            if (removeConfirmed) {
                craftEditor.RemoveParts (toBeRemoved);
                ui.DisplayUserMessage ($"{toBeRemoved.Count} parts removed");
            }

            return removeConfirmed;
        }

        private List<KspPartObject> FindRemovedAndDependentParts (KspCraftObject craft, RegexFilter filter)
        {
            ui.DisplayUserMessage ($"Entering craft '{craft.Name}'...");
            var partLookup = new PartLookup (craft);
            var removedParts = partLookup.LookupParts (filter).ToList ();
            ui.DisplayUserMessage ($"Found {removedParts.Count} parts to be removed");

            var dependentParts = new HashSet<KspPartObject> ();
            Parallel.ForEach (removedParts, removedPart => {
                foreach (var part in partLookup.LookupHardDependencies (removedPart).Except (removedParts)) {
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

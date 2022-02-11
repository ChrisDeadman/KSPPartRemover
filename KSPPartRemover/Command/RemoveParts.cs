using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;
using System.Threading.Tasks;

namespace KSPPartRemover.Command
{
    public class RemoveParts
    {
        private readonly ProgramUI ui;

        public RemoveParts(ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute(String inputFilePath, String outputFilePath, RegexFilter craftFilter, RegexFilter partFilter)
        {
            ui.DisplayUserMessage($"Searching for crafts matching '{craftFilter}'...");

            var kspObjTree = CraftLoader.LoadFromFile(inputFilePath);
            var crafts = new CraftLookup(kspObjTree).LookupCrafts(craftFilter).ToList();

            if (crafts.Count <= 0) {
                ui.DisplayErrorMessage($"No craft matching '{craftFilter}' found, aborting");
                return -1;
            }

            ui.DisplayUserMessage($"Searching for parts matching '{partFilter}'...");

            var partsRemoved = crafts.Aggregate(false, (removed, craft) => removed | RemoveMatchingParts(craft, partFilter));

            if (!partsRemoved) {
                ui.DisplayErrorMessage($"No parts removed");
                return -1;
            }

            CraftLoader.SaveToFile(outputFilePath, kspObjTree);

            return 0;
        }

        private bool RemoveMatchingParts(KspCraftObject craft, RegexFilter partFilter)
        {
            var toBeRemoved = FindRemovedAndDependentParts(craft, partFilter);
            if (toBeRemoved.Count <= 0) {
                return false;
            }

            ui.DisplayUserList("Removed Parts", toBeRemoved.Select(part => ProgramUI.PartObjectToString(craft, part)));

            var removeConfirmed = ui.AskYesNoQuestion("Remove the listed parts?");

            if (removeConfirmed) {
                craft.Edit().RemoveParts(toBeRemoved);

                ui.DisplayUserMessage($"{toBeRemoved.Count} parts removed");
            }

            return removeConfirmed;
        }

        private List<KspPartObject> FindRemovedAndDependentParts(KspCraftObject craft, RegexFilter filter)
        {
            ui.DisplayUserMessage($"Entering craft '{craft.Name}'...");

            var partLookup = new PartLookup(craft);

            var removedParts = partLookup.LookupParts(filter).ToList();

            ui.DisplayUserMessage($"Found {removedParts.Count} parts to be removed");

            var dependentParts = new HashSet<KspPartObject>();

            Parallel.ForEach(removedParts, removedPart => {
                foreach (var part in partLookup.LookupHardDependencies(removedPart).Except(removedParts)) {
                    lock (dependentParts) {
                        dependentParts.Add(part);
                    }
                }
            });

            ui.DisplayUserMessage($"Found {dependentParts.Count} dependent parts");

            return removedParts.Concat(dependentParts).ToList();
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;
using System.Threading.Tasks;

namespace KSPPartRemover.Command
{
    public class ListPartDeps
    {
        private readonly ProgramUI ui;

        public ListPartDeps(ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute(String inputFilePath, RegexFilter craftFilter, RegexFilter partFilter)
        {
            ui.DisplayUserMessage($"Searching for crafts matching '{craftFilter}'...");

            var kspObjTree = ObjectLoader.LoadFromFile(inputFilePath);
            var crafts = new CraftLookup(kspObjTree).LookupCrafts(craftFilter);

            ui.DisplayUserMessage($"Searching for parts with dependencies to '{partFilter}'...");

            var partDependencies = crafts.ToDictionary(craft => craft, craft => FindPartDependencies(craft, partFilter));

            if (partDependencies.Any(entry => entry.Value.Count > 0)) {
                ui.DisplayPartDependencyList(partDependencies);
            }

            return 0;
        }

        private Dictionary<KspPartObject, List<KspPartLinkProperty>> FindPartDependencies(KspCraftObject craft, RegexFilter filter)
        {
            ui.DisplayUserMessage($"Entering craft '{craft.Name}'...");

            var partLookup = new PartLookup(craft);
            var dependencies = partLookup.LookupParts(filter).ToList();

            var dependentParts = new HashSet<KspPartObject>();
            Parallel.ForEach(dependencies, dependency => {
                foreach (var part in partLookup.LookupSoftDependencies(dependency)) {
                    lock (dependentParts) {
                        dependentParts.Add(part);
                    }
                }
            });

            ui.DisplayUserMessage($"Found {dependentParts.Count} dependent parts");

            return dependentParts.ToDictionary(part => part, part => FindPartLinks(part, dependencies));
        }

        private List<KspPartLinkProperty> FindPartLinks(KspPartObject part, List<KspPartObject> dependencies)
        {
            return part.Properties.OfType<KspPartLinkProperty>().Where(link => dependencies.Contains(link.Part)).ToList();
        }
    }
}

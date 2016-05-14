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
    public class ListPartDeps
    {
        private readonly ProgramUI ui;

        public ListPartDeps (ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute (Parameters parameters)
        {
            var allCrafts = CraftLoader.Load (parameters.InputText);
            ui.DisplayUserMessage ($"Searching for crafts matching '{parameters.CraftFilter}'...");
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name);
            ui.DisplayUserMessage ($"Searching for parts with dependencies to '{parameters.PartFilter}'...");
            var partDependencies = filteredCrafts.ToDictionary (craft => craft, craft => FindPartDependencies (craft, parameters.PartFilter));

            ui.DisplayPartDependencyList (partDependencies);

            return 0;
        }

        private IReadOnlyDictionary<KspPartObject, List<KspPartLinkProperty>> FindPartDependencies (KspCraftObject craft, RegexFilter filter)
        {
            ui.DisplayUserMessage ($"Entering craft '{craft.Name}'...");
            var partLookup = new PartLookup (craft);
            var dependencies = partLookup.LookupParts (filter).ToList ();

            var dependentParts = new HashSet<KspPartObject> ();
            Parallel.ForEach (dependencies, dependency => {
                foreach (var part in partLookup.LookupSoftDependencies (dependency)) {
                    lock (dependentParts) {
                        dependentParts.Add (part);
                    }
                }
            });
            ui.DisplayUserMessage ($"Found {dependentParts.Count} dependent parts");

            return dependentParts.ToDictionary (part => part, part => FindPartLinks (part, dependencies));
        }

        private List<KspPartLinkProperty> FindPartLinks (KspPartObject part, List<KspPartObject> dependencies)
        {
            return part.Properties.OfType<KspPartLinkProperty> ().Where (link => dependencies.Contains (link.Part)).ToList ();
        }
    }
}

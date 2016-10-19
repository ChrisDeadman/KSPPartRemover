using System;
using System.Collections.Generic;
using KSPPartRemover.Feature;
using KSPPartRemover.Command;

namespace KSPPartRemover
{
    public class Program
    {
        public static int Main (params String[] args)
        {
            var errors = new List<String> ();
            var parameters = new Parameters () {
                CraftFilter = new RegexFilter (""),
                PartFilter = new RegexFilter ("")
            };

            var ui = new ProgramUI (parameters);
            var printInfoHeader = new Info (ui);
            var printUsage = new Help (ui);

            var commandLineParser = new CommandLineParser ()
                .RequiredArgument (0, cmd => parameters.Command = ParseCommand (cmd, ui, parameters))
                .RequiredSwitchArg<String> ("i", filePath => parameters.InputFilePath = filePath)
                .OptionalSwitchArg<String> ("o", filePath => parameters.OutputFilePath = filePath)
                .OptionalSwitchArg<String> ("c", "craft", pattern => parameters.CraftFilter = new RegexFilter (pattern))
                .OptionalSwitchArg<String> ("p", "part", pattern => parameters.PartFilter = new RegexFilter (pattern))
                .OptionalSwitch ("s", "silent", () => parameters.SilentExecution = true)
                .OnError (errors.Add);
            
            try {
                commandLineParser.Parse (args);

                if (parameters.OutputFilePath == null) {
                    parameters.OutputFilePath = parameters.InputFilePath;
                }

                if (errors.Count > 0) {
                    printInfoHeader.Execute ();
                    printUsage.Execute ();
                    errors.ForEach (ui.DisplayErrorMessage);
                    return -1;
                }

                if (!parameters.SilentExecution)
                    printInfoHeader.Execute ();

                return parameters.Command ();
            } catch (Exception ex) {
                ui.DisplayErrorMessage (ex.ToString ());
                return -1;
            }
        }

        private static Func<int> ParseCommand (String cmd, ProgramUI ui, Parameters parameters)
        {
            switch (cmd) {
            case "list-crafts":
                return () => new ListCrafts (ui).Execute (parameters.InputFilePath, parameters.CraftFilter);
            case "list-parts":
                return () => new ListParts (ui).Execute (parameters.InputFilePath, parameters.CraftFilter, parameters.PartFilter);
            case "list-partdeps":
                return () => new ListPartDeps (ui).Execute (parameters.InputFilePath, parameters.CraftFilter, parameters.PartFilter);
            case "remove-parts":
                return () => new RemoveParts (ui).Execute (parameters.InputFilePath, parameters.OutputFilePath, parameters.CraftFilter, parameters.PartFilter);
            default:
                throw new ArgumentException ($"Invalid command '{cmd}'...");
            }
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using KSPPartRemover.KspFormat;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;

namespace KSPPartRemover
{
    public class Program
    {
        private readonly Parameters parameters;

        public Program(Parameters parameters)
        {
            this.parameters = parameters;
        }

        private static void PrintInfoHeader ()
        {
            var assemblyName = typeof(Program).Assembly.GetName ();

            var infoHeader = new StringBuilder ();
            infoHeader.AppendLine ($"{assemblyName.Name} v{assemblyName.Version}");
            infoHeader.AppendLine ("Compatible with KSP version: 1.1");

            Console.WriteLine (infoHeader.ToString ());
        }

        private static void PrintUsage ()
        {
            var assemblyName = typeof(Program).Assembly.GetName ();

            Console.Write ("usage: ");
            Console.Write (Path.GetFileName (assemblyName.CodeBase));
            Console.WriteLine (" <command> [<args>] -i <input-file> [-o <output-file>]");
            Console.WriteLine ();
            Console.WriteLine ("Commands:"); 
            Console.WriteLine (); 
            Console.WriteLine ("\t remove-parts");
            Console.WriteLine ("\t\t remove one or more parts from one or more crafts");
            Console.WriteLine ();
            Console.WriteLine ("\t list-crafts");
            Console.WriteLine ("\t\t list crafts in the input file");
            Console.WriteLine ();
            Console.WriteLine ("\t list-parts");
            Console.WriteLine ("\t\t list parts in the input file");
            Console.WriteLine ();
            Console.WriteLine ("\t list-partdeps");
            Console.WriteLine ("\t\t same as 'list-parts' but also prints dependencies");
            Console.WriteLine ("\t\t the part filter is applied on the dependencies for this command");
            Console.WriteLine ();
            Console.WriteLine ("Switches:"); 
            Console.WriteLine (); 
            Console.WriteLine ("\t -i <path>");
            Console.WriteLine ("\t\t specifies the input file");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -o <path>");
            Console.WriteLine ("\t\t specifies the output file (prints to stdout if not specified)");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -c, --craft <name-pattern>");
            Console.WriteLine ("\t\t apply craft filter (applies to all crafts if not specified)");
            Console.WriteLine ("\t\t '!' in front of the regex performs inverse matching");
            Console.WriteLine ("\t\t example for name pattern: --craft \'!^Asteroid\'");
            Console.WriteLine ("\t\t example for inverse matching: --craft \'!Mün\'");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -p, --part <id or name-pattern>");
            Console.WriteLine ("\t\t apply part filter (applies to all parts if not specified)");
            Console.WriteLine ("\t\t '!' in front of the regex performs inverse matching");
            Console.WriteLine ("\t\t example for id: --part 1");
            Console.WriteLine ("\t\t example for name pattern: --part \'fuelTank.*\'");
            Console.WriteLine ("\t\t example for inverse matching: --part \'!^PotatoRoid$\'");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -s, --silent");
            Console.WriteLine ("\t\t do not print additional info, do not ask for confirmation");
            Console.WriteLine ();
        }

        public static int Main (params String[] args)
        {
            var parameters = new Parameters() {
                CraftFilter = new RegexFilter(""),
                PartFilter = new RegexFilter(""),
                OutputTextWriter = Console.Out
            };

            var program = new Program (parameters);

            var errors = new List<String> ();

            var commandLineParser = new CommandLineParser().
                Command(cmd => {switch(cmd) {
                case "list-crafts":
                    parameters.Command = program.ListCrafts;
                    break;
                case "list-parts":
                    parameters.Command = program.ListParts;
                    break;
                case "list-partdeps":
                    parameters.Command = program.ListPartDeps;
                    break;
                case "remove-parts":
                    parameters.Command = program.PerformRemovePartCommand;
                    break;
                default:
                    errors.Add ($"Invalid command '{cmd}'...");
                    break;
                }}).
                Required<String>("-i", fileName => parameters.InputText = ReadInputFile (fileName)).
                Optional<String>("-o", fileName => parameters.OutputTextWriter = OpenOutputFile(fileName)).
                Optional<String>("-c", pattern => parameters.CraftFilter = new RegexFilter(pattern)).
                Optional<String>("--craft", pattern => parameters.CraftFilter = new RegexFilter(pattern)).
                Optional<String>("-p", pattern => parameters.PartFilter = new RegexFilter(pattern)).
                Optional<String>("--part", pattern => parameters.PartFilter = new RegexFilter(pattern)).
                Optional("-s", () => parameters.SilentExecution = true).
                Optional("--silent", () => parameters.SilentExecution = true).
                Error(errors.Add);

            try {
                commandLineParser.Parse (args);

                if (errors.Count > 0) {
                    PrintInfoHeader ();
                    PrintUsage ();
                    errors.ForEach(error => Console.WriteLine($"ERROR: {error}"));
                    return -1;
                }

                if (!parameters.SilentExecution)
                    PrintInfoHeader ();

                return parameters.Command ();
            } catch (Exception ex) {
                PrintException (ex);
                return -1;
            } finally {
                if (parameters.OutputTextWriter != Console.Out) {
                    parameters.OutputTextWriter.Dispose ();
                }
            }
        }

        public int ListCrafts ()
        {
            ConsoleWriteLineIfNotSilent ($"Searching for crafts matching '{parameters.CraftFilter}'...");

            var allCrafts = CraftLoader.Load (parameters.InputText);
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name);

            ConsoleWriteLineIfNotSilent ("");
            PrintList (filteredCrafts.Select (craft => craft.Name));

            return 0;
        }

        public int ListParts ()
        {
            ConsoleWriteLineIfNotSilent ($"Searching for crafts matching '{parameters.CraftFilter}'...");

            var allCrafts = CraftLoader.Load (parameters.InputText);
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name);

            ConsoleWriteLineIfNotSilent ("");
            foreach (var craft in filteredCrafts) {
                var matchingParts = new PartLookup(craft).LookupParts(parameters.PartFilter).ToList();
                if (matchingParts.Count > 0) {
                    Console.WriteLine ($"{craft.Name}:");
                    PrintList (matchingParts.Select (part => $"\t{PartObjectToString (craft, part)}"));   
                }
            }

            return 0;
        }

        public int ListPartDeps ()
        {
            ConsoleWriteLineIfNotSilent ($"Searching for crafts matching '{parameters.CraftFilter}'...");

            var allCrafts = CraftLoader.Load (parameters.InputText);
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name);

            ConsoleWriteLineIfNotSilent ("");
            foreach (var craft in filteredCrafts) {
                Console.WriteLine ($"{craft.Name}:");

                var matchingParts = new PartLookup(craft).LookupParts(parameters.PartFilter).ToList();
                var dependentParts = matchingParts.SelectMany (part => new PartLookup(craft).LookupSoftDependencies (part)).Distinct ();

                foreach (var part in dependentParts) {
                    var links = part.Properties.OfType<KspPartLinkProperty> ().Where (link => matchingParts.Contains (link.Part));
                    Console.WriteLine ($"\t{PartObjectToString (craft, part)}:");
                    PrintList (links.Select (link => ($"\t\t{PartLinkPropertyToString (craft, link)}")));
                }
            }

            return 0;
        }

        public int PerformRemovePartCommand ()
        {
            ConsoleWriteLineIfNotSilent ($"Searching for crafts matching '{parameters.CraftFilter}'...");

            KspObject kspObjTree;
            var allCrafts = CraftLoader.Load (parameters.InputText, out kspObjTree);
            var filteredCrafts = parameters.CraftFilter.Apply (allCrafts, craft => craft.Name).ToList();

            if (filteredCrafts.Count <= 0) { 
                Console.WriteLine ($"No craft matching '{parameters.CraftFilter}' found, aborting"); 
                return -1;
            } 

            var removedPartCount = 0;

            foreach (var craft in filteredCrafts) {
                if (!parameters.SilentExecution) {
                    Console.WriteLine ($"Entering craft '{craft.Name}'");
                }

                ConsoleWriteLineIfNotSilent ($"Searching for parts matching '{parameters.PartFilter}'...");

                var matchingParts = new PartLookup(craft).LookupParts(parameters.PartFilter).ToList();
                var dependentParts = matchingParts.SelectMany (part => new PartLookup(craft).LookupHardDependencies (part)).Distinct ();
                var removedParts = matchingParts.Concat (dependentParts).Distinct ().ToArray ();

                if (removedParts.Length <= 0) {
                    ConsoleWriteLineIfNotSilent ($"No parts matching '{parameters.PartFilter}' found");
                    continue;
                }

                if (!parameters.SilentExecution) {
                    Console.WriteLine ();
                    Console.WriteLine ("The following parts will be removed:");
                    Console.WriteLine ("====================================");
                    PrintList (removedParts.Select (part => PartObjectToString(craft, part)));
                    Console.WriteLine ("====================================");
                    if (!ConfirmPartRemoval ()) {
                        continue;
                    } else {
                        Console.WriteLine ();
                    }
                }

                craft.Edit ().RemoveParts (removedParts);
                removedPartCount += removedParts.Length;
            }

            var craftToken = KspObjectWriter.WriteObject (kspObjTree);
            var craftString = KspTokenWriter.WriteToken(craftToken, new StringBuilder()).ToString();
            parameters.OutputTextWriter.Write (craftString);

            return (removedPartCount > 0) ? 0 : -1;
        }

        private void PrintList (IEnumerable<String> entries)
        {
            const int entriesPerPage = 20;

            var currentEntry = 1;
            foreach (var entry in entries) {
                if (currentEntry++ % entriesPerPage == 0)
                if (!parameters.SilentExecution) {
                    Console.WriteLine ();
                    Console.Write ("Press any key for next page...");
                    Console.ReadKey ();
                    Console.WriteLine ();
                }
                Console.WriteLine (entry);
            }
        }

        private void ConsoleWriteIfNotSilent (String value)
        {
            if (parameters.SilentExecution)
                return;

            Console.Write (value);
        }

        private void ConsoleWriteLineIfNotSilent (String value = null)
        {
            if (parameters.SilentExecution)
                return;

            Console.WriteLine (value ?? String.Empty);
        }

        private static String ReadInputFile(String fileName)
        {
            using (var inputTextReader = new StreamReader (new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8))
                return inputTextReader.ReadToEnd ();
        }

        private static TextWriter OpenOutputFile(String fileName) => new StreamWriter (new FileStream (fileName, FileMode.Truncate, FileAccess.ReadWrite, FileShare.Read), Encoding.UTF8);

        private static bool ConfirmPartRemoval ()
        {
            Console.Write ("Perform the removal? (y/[N]) ");
            var answeredYes = Console.ReadKey ().Key == ConsoleKey.Y;
            Console.WriteLine ();

            return answeredYes;
        }

        private static void PrintException (Exception exception)
        {
            Console.WriteLine ();
            Console.Write ("EXCEPTION: ");
            Console.WriteLine (exception.ToString ());
            Console.WriteLine ();
        }

        private static String PartObjectToString (KspCraftObject craft, KspPartObject part) => $"[{craft.IdOfChild (part)}]{part.Name}";

        private static String PartLinkPropertyToString (KspCraftObject craft, KspPartLinkProperty reference)
        {
            var sb = new StringBuilder ();
            sb.Append ($"{PartObjectToString (craft, reference.Part)}");
            sb.Append ($"[{reference.Name}");
            if (reference.Prefix != null) {
                sb.Append ($"({reference.Prefix})");
            }
            sb.Append("]");
            return sb.ToString ();
        }
    }
}

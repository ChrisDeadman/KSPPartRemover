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
    public static class Program
    {
        private static Func<int> commandHandler;
        private static String partNamePattern;
        private static String inputText;
        private static TextWriter outputTextWriter;
        private static String craftNameRegex;
        private static bool silentExecution;

        private static readonly Dictionary<String, Func<int, String[], int>> ArgumentParsers =
            new Dictionary<String, Func<int, String[], int>> {
                { "remove-part", ParseRemoveCommand },
                { "list-crafts", ParseListCraftsCommand },
                { "list-parts", ParseListPartsCommand },
                { "list-partdeps", ParseListPartDepsCommand },
                { "-i", ParseInputFilePathArgument },
                { "-o", ParseOutputFilePathArgument },
                { "-c", ParseCraftNameRegexArgument },
                { "--craft", ParseCraftNameRegexArgument },
                { "-p", ParsePartNamePatternArgument },
                { "--part", ParsePartNamePatternArgument },
                { "-s", ParseSilentExecutionArgument },
                { "--silent", ParseSilentExecutionArgument }
            };

        private static int ParseRemoveCommand (int argIdx, params String[] args) => ParseCommand(argIdx, PerformRemovePartCommand);
        private static int ParseListCraftsCommand (int argIdx, params String[] args) => ParseCommand(argIdx, PerformListCraftsCommand);
        private static int ParseListPartsCommand (int argIdx, params String[] args) => ParseCommand(argIdx, PerformListPartsCommand);
        private static int ParseListPartDepsCommand (int argIdx, params String[] args) => ParseCommand(argIdx, PerformListPartDepsCommand);

        private static void PrintInfoHeader ()
        {
            var assemblyName = typeof(Program).Assembly.GetName ();

            var infoHeader = new StringBuilder ();
            infoHeader.AppendFormat ("{0} v{1}", assemblyName.Name, assemblyName.Version);
            infoHeader.AppendLine ();   
            infoHeader.Append ("Compatible with KSP version: 1.1");

            Console.WriteLine ();
            Console.WriteLine (infoHeader.ToString ());
            Console.WriteLine ();
        }

        private static void PrintUsage ()
        {
            var assemblyName = typeof(Program).Assembly.GetName ();

            Console.Write ("usage: ");
            Console.Write (Path.GetFileName (assemblyName.CodeBase));
            Console.WriteLine (" <command> [<args>] -i <inputFilePath> [-o <outputFilePath>]");
            Console.WriteLine ();
            Console.WriteLine ("Commands:"); 
            Console.WriteLine (); 
            Console.WriteLine ("\t remove-part");
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
            Console.WriteLine ("\t -i <FilePath>");
            Console.WriteLine ("\t\t specifies the input file");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -o <FilePath>");
            Console.WriteLine ("\t\t specifies the output file (prints to stdout if not specified)");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -c, --craft <craftNameRegex>");
            Console.WriteLine ("\t\t apply craft filter (applies to all crafts if not specified)");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -p, --part <partId or partNameRegex>");
            Console.WriteLine ("\t\t apply part filter (applies to all parts if not specified)");
            Console.WriteLine ();
            Console.WriteLine ("\t[Optional]");
            Console.WriteLine ("\t -s, --silent");
            Console.WriteLine ("\t\t do not print additional info, do not ask for confirmation");
            Console.WriteLine ();
        }

        private static void SetDefaultArguments ()
        {
            commandHandler = null;
            partNamePattern = null;
            inputText = null;
            outputTextWriter = Console.Out;
            craftNameRegex = null;
            silentExecution = false;
        }

        public static int Main (params String[] args)
        {
            try {
                try {
                    SetDefaultArguments ();
                    ParseArguments (args);
                } catch (ArgumentException ex) {
                    PrintInfoHeader ();
                    PrintUsage ();
                    OutputExceptionBrief (ex);
                    return -1;
                } catch (FileNotFoundException ex) {
                    PrintInfoHeader ();
                    OutputExceptionBrief (ex);
                    return -1;
                } catch (Exception ex) {
                    PrintInfoHeader ();
                    OutputExceptionDetailed (ex);
                    return -1;
                }

                try {
                    if (!silentExecution)
                        PrintInfoHeader ();

                    if (commandHandler == null)
                        throw new ArgumentException ("No command specified");

                    if (inputText == null)
                        throw new ArgumentException ("No input file specified");

                    return commandHandler ();
                } catch (ArgumentException ex) {
                    PrintUsage ();
                    OutputExceptionBrief (ex);
                    return -1;
                } catch (KeyNotFoundException ex) {
                    OutputExceptionBrief (ex);
                    return -1;
                } catch (Exception ex) {
                    OutputExceptionDetailed (ex);
                    return -1;
                }
            } finally {
                if (outputTextWriter != Console.Out)
                    outputTextWriter.Dispose ();
            }
        }

        private static int PerformListCraftsCommand ()
        {
            var kspObjTree = KspObjectReader.ReadObject (KspTokenReader.ReadToken(inputText));
            var crafts = Crafts (kspObjTree, craftNameRegex);

            ConsoleWriteLineIfNotSilent ("");
            PrintList (crafts.Select (craft => craft.Name));

            return 0;
        }

        private static int PerformListPartsCommand ()
        {
            var kspObjTree = KspObjectReader.ReadObject (KspTokenReader.ReadToken(inputText));
            var crafts = Crafts (kspObjTree, craftNameRegex);

            ConsoleWriteLineIfNotSilent ("");
            foreach (var craft in crafts) {
                var matchingParts = Parts (craft, partNamePattern);
                if (matchingParts.Count > 0) {
                    Console.WriteLine ($"{craft.Name}:");
                    PrintList (matchingParts.Select (part => $"\t{PartObjectToString (craft, part)}"));   
                }
            }

            return 0;
        }

        private static int PerformListPartDepsCommand ()
        {
            var kspObjTree = KspObjectReader.ReadObject (KspTokenReader.ReadToken(inputText));
            var crafts = Crafts (kspObjTree, craftNameRegex);

            ConsoleWriteLineIfNotSilent ("");
            foreach (var craft in crafts) {
                Console.WriteLine ($"{craft.Name}:");

                foreach (var part in Parts(craft)) {
                    var matchingPartLinks = PartLinks(craft, part, partNamePattern);
                    if (matchingPartLinks.Count > 0) {
                        Console.WriteLine ($"\t{PartObjectToString (craft, part)}:");
                        PrintList (matchingPartLinks.Select (link => ($"\t\t{PartLinkPropertyToString (craft, link)}")));
                    }
                }
            }

            return 0;
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

        private static int PerformRemovePartCommand ()
        {
            if (String.IsNullOrEmpty (partNamePattern)) {
                throw new ArgumentException ("no part specified");
            }

            var kspObjTree = KspObjectReader.ReadObject (KspTokenReader.ReadToken(inputText));
            var crafts = Crafts (kspObjTree, craftNameRegex);

            foreach (var craft in crafts) {
                if (!silentExecution) {
                    Console.WriteLine ($"Entering craft '{craft.Name}'");
                }

                ConsoleWriteLineIfNotSilent ($"Searching for parts matching '{partNamePattern}'...");

                var matchingParts = Parts (craft, partNamePattern);
                var dependentParts = matchingParts.SelectMany (part => PartLookup.EvaluateHardDependencies (craft, part)).Distinct ();
                var removedParts = matchingParts.Concat (dependentParts).Distinct ().ToArray ();

                if (removedParts.Length <= 0)
                    throw new KeyNotFoundException ($"No parts matching '{partNamePattern}' found, aborting.");
                
                if (!silentExecution) {
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
            }

            var craftToken = KspObjectWriter.WriteObject (kspObjTree);
            var craftString = KspTokenWriter.WriteToken(craftToken, new StringBuilder()).ToString();
            outputTextWriter.Write (craftString);

            return 0;
        }

        private static IReadOnlyList<KspCraftObject> Crafts (KspObject kspObjTree, String craftNameRegex)
        {
            ConsoleWriteLineIfNotSilent ($"Searching for crafts matching '{craftNameRegex}'...");
            var crafts = kspObjTree.Children <KspCraftObject> (recursive: true).ToList ();
            if (kspObjTree is KspCraftObject) {
                crafts.Add (kspObjTree as KspCraftObject);
            }

            if (!String.IsNullOrEmpty (craftNameRegex)) {
                crafts = crafts.Where (craft => MatchesRegex (craft.Name, craftNameRegex)).ToList ();
                if (crafts.Count <= 0) {
                    throw new KeyNotFoundException ($"No craft matching '{craftNameRegex}' found, aborting.");
                }
            }

            return crafts;
        }

        private static IReadOnlyList<KspPartObject> Parts (KspCraftObject craft, String partNamePattern = null)
        {
            int id;
            List<KspPartObject> dependencies;

            if (String.IsNullOrEmpty (partNamePattern)) {
                dependencies = craft.Children <KspPartObject> ().ToList ();
            }
            else if (int.TryParse (partNamePattern, out id)) {
                dependencies = new List<KspPartObject>();
                var dependency = craft.Child<KspPartObject>(id);
                if (dependency != null) {
                    dependencies.Add(dependency);
                }
            }
            else {
                dependencies = craft.Children <KspPartObject> ().Where (part => MatchesRegex (part.Name, partNamePattern)).ToList ();
            }

            return dependencies;
        }

        private static IReadOnlyList<KspPartLinkProperty> PartLinks (KspCraftObject craft, KspPartObject part, String partNamePattern = null)
        {
            return PartLinks(part, Parts(craft, partNamePattern));
        }

        private static IReadOnlyList<KspPartLinkProperty> PartLinks (KspPartObject part, IReadOnlyList<KspPartObject> dependencies) =>
            part.Properties.OfType<KspPartLinkProperty> ().Where (link => dependencies.Any(dep => Object.Equals(link.Part, dep))).ToList();

        private static void PrintList (IEnumerable<String> entries)
        {
            const int entriesPerPage = 50;

            var currentEntry = 1;
            foreach (var entry in entries) {
                if (currentEntry++ % entriesPerPage == 0)
                if (!silentExecution) {
                    Console.WriteLine ();
                    Console.Write ("Press any key for next page...");
                    Console.ReadKey ();
                    Console.WriteLine ();
                }
                Console.WriteLine (entry);
            }
        }

        private static bool ConfirmPartRemoval ()
        {
            Console.Write ("Perform the removal? (y/[N]) ");
            var answeredYes = Console.ReadKey ().Key == ConsoleKey.Y;
            Console.WriteLine ();

            return answeredYes;
        }

        private static void ConsoleWriteIfNotSilent (String value)
        {
            if (silentExecution)
                return;

            Console.Write (value);
        }

        private static void ConsoleWriteLineIfNotSilent (String value = null)
        {
            if (silentExecution)
                return;

            Console.WriteLine (value ?? String.Empty);
        }

        private static int ParseCommand (int argIdx, Func<int> handler)
        {
            if (commandHandler != null)
                throw new ArgumentException ("");

            commandHandler = handler;
            return argIdx;
        }

        private static void ParseArguments (params String[] args)
        {
            for (var argIdx = 0; argIdx < args.Length; argIdx++) {
                Func<int, String[], int> parser;
                if (!ArgumentParsers.TryGetValue (args [argIdx], out parser))
                    throw new ArgumentException ("Illegal argument", args [argIdx]);

                argIdx = parser (argIdx, args);
            }
        }

        private static int ParseInputFilePathArgument (int argIdx, params String[] args)
        {
            argIdx++;
            if (args.Length <= argIdx)
                throw new ArgumentException ("");

            using (var inputTextReader = new StreamReader (new FileStream (args [argIdx], FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8))
                inputText = inputTextReader.ReadToEnd ();

            return argIdx;
        }

        private static int ParseOutputFilePathArgument (int argIdx, params String[] args)
        {
            argIdx++;
            if (args.Length <= argIdx)
                throw new ArgumentException ("");

            outputTextWriter = new StreamWriter (new FileStream (args [argIdx], FileMode.Truncate, FileAccess.ReadWrite, FileShare.Read), Encoding.UTF8);
            return argIdx;
        }

        private static int ParseCraftNameRegexArgument (int argIdx, params String[] args)
        {
            argIdx++;
            if (args.Length <= argIdx)
                throw new ArgumentException ("");

            craftNameRegex = args [argIdx];
            return argIdx;
        }

        private static int ParsePartNamePatternArgument (int argIdx, params String[] args)
        {
            argIdx++;
            if (args.Length <= argIdx)
                throw new ArgumentException ("");

            partNamePattern = args [argIdx].Trim ('\"');
            return argIdx;
        }

        private static int ParseSilentExecutionArgument (int argIdx, params String[] args)
        {
            silentExecution = true;
            return argIdx;
        }

        private static void OutputExceptionBrief (Exception exception)
        {
            var message = exception.Message;
            if (String.IsNullOrEmpty (message))
                message = "Invalid command line arguments.";
            
            Console.Write ("ERROR: ");
            Console.WriteLine (message);
            Console.WriteLine ();
        }

        private static void OutputExceptionDetailed (Exception exception)
        {
            Console.WriteLine ();
            Console.Write ("EXCEPTION: ");
            Console.WriteLine (exception.ToString ());
            Console.WriteLine ();
        }

        private static bool MatchesRegex (String str, String pattern)
        {
            return String.IsNullOrEmpty (pattern) || Regex.Match (str, pattern).Success;
        }
    }
}

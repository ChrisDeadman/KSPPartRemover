using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using KSPPartRemover.KspObjects;
using KSPPartRemover.KspObjects.Format;
using KSPPartRemover.Extension;
using KSPPartRemover.Feature;

namespace KSPPartRemover
{
    public static class Program
    {
        private enum Command
        {
            Unspecified,
            ListCrafts,
            ListParts,
            RemovePart
        }

        private static Command command;
        private static String partNamePattern;
        private static String inputText;
        private static TextWriter outputTextWriter;
        private static String craftNamePattern;
        private static bool silentExecution;

        private static readonly Dictionary<String, Func<int, String[], int>> ArgumentParsers =
            new Dictionary<String, Func<int, String[], int>> {
                { "remove-part", ParseRemoveCommand },
                { "list-crafts", ParseListCraftsCommand },
                { "list-parts", ParseListPartsCommand },
                { "-i", ParseInputFilePathArgument },
                { "-o", ParseOutputFilePathArgument },
                { "-c", ParseCraftNameArgument },
                { "--craft", ParseCraftNameArgument },
                { "-s", ParseSilentExecutionArgument },
                { "--silent", ParseSilentExecutionArgument }
            };

        private static readonly Dictionary<Command, Func<int>> CommandDelegates =
            new Dictionary<Command, Func<int>> {
                { Command.ListCrafts, PerformListCraftsCommand },
                { Command.ListParts, PerformListPartsCommand },
                { Command.RemovePart, PerformRemovePartCommand }
            };

        private static void PrintInfoHeader ()
        {
            var assemblyName = typeof(Program).Assembly.GetName ();

            var infoHeader = new StringBuilder ();
            infoHeader.AppendFormat ("{0} v{1}", assemblyName.Name, assemblyName.Version);
            infoHeader.AppendLine ();
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
            Console.WriteLine (" <COMMAND> [-s | --silent] -i <inputFilePath> [-o <outputFilePath>]");
            Console.WriteLine ();
            Console.WriteLine ("Commands:");
            Console.WriteLine ();
            Console.WriteLine ("\t remove-part <partId or partNamePattern>");
            Console.WriteLine ("\t\t 'partId': remove the part with the given id (integer);");
            Console.WriteLine ("\t\t 'partNamePattern': remove all parts matching the given regex pattern;");
            Console.WriteLine ();
            Console.WriteLine ("\t list-crafts");
            Console.WriteLine ("\t\t list all crafts in the input file;");
            Console.WriteLine ();
            Console.WriteLine ("\t list-parts");
            Console.WriteLine ("\t\t list all parts in the input file;");
            Console.WriteLine ();
            Console.WriteLine ("Arguments:");
            Console.WriteLine ();
            Console.WriteLine ("\t -i <FilePath>");
            Console.WriteLine ("\t\t specifies the input file;");
            Console.WriteLine ();
            Console.WriteLine ("\t -o <FilePath>");
            Console.WriteLine ("\t\t [Optional] specifies the output file");
            Console.WriteLine ("\t\t            prints to stdout if not specified;");
            Console.WriteLine ();
            Console.WriteLine ("\t -c, --craft <craftNamePattern>");
            Console.WriteLine ("\t\t [Optional] regex pattern for target craft(s);");
            Console.WriteLine ("\t\t            processes all crafts if not specified;");
            Console.WriteLine ("\t\t            applies to all commands;");
            Console.WriteLine ();
            Console.WriteLine ("\t -s, --silent");
            Console.WriteLine ("\t\t [Optional] do not print additional info;");
            Console.WriteLine ("\t\t            do not ask for confirmation;");
            Console.WriteLine ();
        }

        private static void SetDefaultArguments ()
        {
            command = Command.Unspecified;
            partNamePattern = null;
            inputText = null;
            outputTextWriter = Console.Out;
            craftNamePattern = null;
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

                    if (command == Command.Unspecified)
                        throw new ArgumentException ("No command specified");

                    if (inputText == null)
                        throw new ArgumentException ("No input file specified");

                    return CommandDelegates [command] ();
                } catch (ArgumentException ex) {
                    PrintUsage ();
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
            var kspObjTree = KspObjectReader.ReadProtoObject (inputText);
            var crafts = Crafts (kspObjTree, craftNamePattern);

            ConsoleWriteLineIfNotSilent ("");
            PrintList (crafts.Select (craft => craft.Name));

            return 0;
        }

        private static int PerformListPartsCommand ()
        {
            var kspObjTree = KspObjectReader.ReadProtoObject (inputText);
            var crafts = Crafts (kspObjTree, craftNamePattern);

            ConsoleWriteLineIfNotSilent ("");
            foreach (var craft in crafts) {
                Console.WriteLine ($"{craft.Name}:");
                PrintList (craft.Children<KspPartObject> ().Select (part => $"\t[{craft.IdOfChild (part)}] {part.Name}"));
            }

            return 0;
        }

        private static int PerformRemovePartCommand ()
        {
            if (String.IsNullOrEmpty (partNamePattern))
                throw new ArgumentException ("no part specified");

            var kspObjTree = KspObjectReader.ReadProtoObject (inputText);
            var crafts = Crafts (kspObjTree, craftNamePattern);

            foreach (var craft in crafts) {
                if (!silentExecution) {
                    Console.WriteLine ($"Entering craft '{craft.Name}'");
                }

                int partToRemoveId;
                var matchingParts = int.TryParse (partNamePattern, out partToRemoveId) ? Parts (craft, partToRemoveId) : Parts (craft, partNamePattern);
                var dependentParts = matchingParts.SelectMany (part => PartLookup.EvaluateHardDependencies (craft, part)).Distinct ();
                var removedParts = matchingParts.Concat (dependentParts).Distinct ().ToArray ();

                if (!silentExecution) {
                    Console.WriteLine ();
                    Console.WriteLine ("The following parts will be removed:");
                    Console.WriteLine ("====================================");
                    PrintList (removedParts.Select (part => $"[id={craft.IdOfChild (part)}] {part.Name}"));
                    Console.WriteLine ("====================================");
                    if (!ConfirmPartRemoval ()) {
                        continue;
                    } else {
                        Console.WriteLine ();
                    }
                }

                craft.Edit ().RemoveParts (removedParts);
            }

            outputTextWriter.Write (KspObjectWriter.ToString (kspObjTree));

            return 0;
        }

        private static IReadOnlyList<KspCraftObject> Crafts (KspObject kspObjTree, String craftNamePattern)
        {
            ConsoleWriteLineIfNotSilent ($"Searching for crafts matching '{craftNamePattern}'...");
            var crafts = kspObjTree.Children <KspCraftObject> (recursive: true).ToList ();
            if (kspObjTree is KspCraftObject) {
                crafts.Add (kspObjTree as KspCraftObject);
            }

            if (!String.IsNullOrEmpty (craftNamePattern)) {
                crafts = crafts.Where (craft => MatchesRegex (craft.Name, craftNamePattern)).ToList ();
                if (crafts.Count <= 0) {
                    throw new KeyNotFoundException ($"No craft matching '{craftNamePattern}' found, aborting.");
                }
            }

            return crafts;
        }

        private static IReadOnlyList<KspPartObject> Parts (KspCraftObject craft, int partId)
        {
            ConsoleWriteLineIfNotSilent ($"Searching for part with id={partId}...");
            var part = craft.Child<KspPartObject> (partId);
            if (part == null)
                throw new KeyNotFoundException ($"No part with id={partId} found, aborting.");

            return new List<KspPartObject> (new[] { part });
        }

        private static IReadOnlyList<KspPartObject> Parts (KspCraftObject craft, String partName)
        {
            ConsoleWriteLineIfNotSilent ($"Searching for parts with name '{partName}'...");
            var occurrences = (String.IsNullOrEmpty (partNamePattern))
                ? craft.Children <KspPartObject> ().ToList ()
                : craft.Children <KspPartObject> ().Where (part => MatchesRegex (part.Name, partNamePattern)).ToList ();
            
            if (occurrences.Count <= 0)
                throw new KeyNotFoundException ($"No parts with a name of '{partName}' found, aborting.");

            return occurrences;
        }

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

        private static int ParseRemoveCommand (int argIdx, params String[] args)
        {
            argIdx++;
            if (args.Length <= argIdx)
                throw new ArgumentException ("");

            if (command != Command.Unspecified)
                throw new ArgumentException ("");

            command = Command.RemovePart;

            partNamePattern = args [argIdx].Trim ('\"');
            return argIdx;
        }

        private static int ParseListCraftsCommand (int argIdx, params String[] args)
        {
            if (command != Command.Unspecified)
                throw new ArgumentException ("");

            command = Command.ListCrafts;

            return argIdx;
        }

        private static int ParseListPartsCommand (int argIdx, params String[] args)
        {
            if (command != Command.Unspecified)
                throw new ArgumentException ("");

            command = Command.ListParts;

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

        private static int ParseCraftNameArgument (int argIdx, params String[] args)
        {
            argIdx++;
            if (args.Length <= argIdx)
                throw new ArgumentException ("");

            craftNamePattern = args [argIdx];
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

            Console.WriteLine ();
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

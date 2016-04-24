using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KSPPartRemover.Extension;
using KSPPartRemover.Format;
using KSPPartRemover.Features;

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
		private static string partNamePattern;
		private static string inputText;
		private static TextWriter outputTextWriter;
		//private static string craftNamePattern;
		private static bool silentExecution;

		private static readonly Dictionary<string, Func<int, string[], int>> ArgumentParsers =
			new Dictionary<string, Func<int, string[], int>> {
				{ "remove-part", ParseRemoveCommand },
				{ "list-crafts", ParseListCraftsCommand },
				{ "list-parts", ParseListPartsCommand },
				{ "-i", ParseInputFilePathArgument },
				{ "-o", ParseOutputFilePathArgument },
				//{ "-c", ParseCraftNameArgument },
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
			Console.WriteLine (" COMMAND [-s | --silent] -i <inputFilePath> [-o <outputFilePath>]");
			Console.WriteLine ();
			Console.WriteLine ("Commands:");
			Console.WriteLine ();
			Console.WriteLine ("\t remove-part <partId or partNamePattern>");
			Console.WriteLine ("\t\t remove the part with the given 'partId' (integer) or all parts matching 'partNamePattern' (regex string)");
			Console.WriteLine ();
			Console.WriteLine ("\t list-crafts");
			Console.WriteLine ("\t\t list all crafts in the input file");
			Console.WriteLine ();
			Console.WriteLine ("\t list-parts");
			Console.WriteLine ("\t\t list all parts in the input file");
			Console.WriteLine ();
			Console.WriteLine ("Arguments:");
			Console.WriteLine ();
			Console.WriteLine ("\t -i <FilePath>");
			Console.WriteLine ("\t\t specifies the input file");
			Console.WriteLine ();
			Console.WriteLine ("\t -o <FilePath>");
			Console.WriteLine ("\t\t [Optional] specifies the output file - prints to stdout if not specified");
			Console.WriteLine ();
			Console.WriteLine ("\t -c <craftNamePattern>");
			Console.WriteLine ("\t\t [Optional] regex pattern for target craft(s) - applies to all commands; processes all crafts if not specified");
			Console.WriteLine ();
			Console.WriteLine ("\t -s, --silent");
			Console.WriteLine ("\t\t [Optional] do not print additional information and do not ask for removal confirmation");
			Console.WriteLine ();
		}

		private static void SetDefaultArguments ()
		{
			command = Command.Unspecified;
			partNamePattern = null;
			inputText = null;
			outputTextWriter = Console.Out;
			//craftNamePattern = null;
			silentExecution = false;
		}

		public static int Main (params string[] args)
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
			var kspObjTree = KspObjectReader.Read (inputText);
			var crafts = Crafts (kspObjTree, null);

			ConsoleWriteLineIfNotSilent ("Crafts in file:");
			ConsoleWriteLineIfNotSilent ("===============");
			PrintList (crafts.Select (craft => craft.GetCraftName ()));

			return 0;
		}

		private static int PerformListPartsCommand ()
		{
			var kspObjTree = KspObjectReader.Read (inputText);
			var crafts = Crafts (kspObjTree, null);

			ConsoleWriteLineIfNotSilent ("Parts in file:");
			ConsoleWriteLineIfNotSilent ("==============");
			foreach (var craft in crafts) {
				Console.WriteLine ("{0}:", craft.GetCraftName ());
				PrintList (craft.GetParts ().Select (part => string.Format ("\t{0} (id={1})", part.GetPartName (), craft.GetIdOfPart (part))));
			}

			return 0;
		}

		private static int PerformRemovePartCommand ()
		{
			if (String.IsNullOrEmpty (partNamePattern))
				throw new ArgumentException ("no part specified");

			var kspObjTree = KspObjectReader.Read (inputText);
			var crafts = Crafts (kspObjTree, null);

			foreach (var craft in crafts) {
				int partToRemoveId;
				var matchingParts = int.TryParse (partNamePattern, out partToRemoveId) ? Parts (craft, partToRemoveId) : Parts (craft, partNamePattern);

				var partRemover = new PartRemover (craft);
				var partRemovalActions = matchingParts.AsParallel ().Select (partRemover.PrepareRemovePart);
				var mergedPartRemovalAction = partRemover.CombineRemovalActions (partRemovalActions);

				if (!silentExecution) {
					Console.WriteLine ();
					Console.WriteLine ("Would remove the following parts from {0}:", craft.GetCraftName ());
					Console.WriteLine ("=============================================");
					PrintList (mergedPartRemovalAction.PartsToBeRemoved.Select (part => string.Format ("{0} (id={1})", part.GetPartName (), craft.GetIdOfPart (part))));
					Console.WriteLine ("=============================================");
					if (!ConfirmPartRemoval ()) {
						Console.WriteLine ("aborted.");
						return -1;
					}
				}

				mergedPartRemovalAction.RemoveParts ();
			}

			outputTextWriter.Write (KspObjectWriter.ToString (kspObjTree));

			return 0;
		}

		private static IReadOnlyList<KspObject> Crafts (KspObject kspObjTree, string craftNamePattern)
		{
			ConsoleWriteLineIfNotSilent (string.Format ("Searching for crafts matching '{0}'...", craftNamePattern));
			var occurrences = (craftNamePattern == null) ? kspObjTree.GetCrafts ().ToList () : kspObjTree.FilterCraftsByNamePattern (craftNamePattern).ToList ();
			if (occurrences.Count <= 0)
				throw new ArgumentException (string.Format ("No craft matching '{0}' found, aborting.", craftNamePattern));

			return occurrences;
		}

		private static IReadOnlyList<KspObject> Parts (KspObject craft, int partId)
		{
			ConsoleWriteLineIfNotSilent (string.Format ("Searching for part with id={0}...", partId));
			var part = craft.GetPartById (partId);
			if (part == null)
				throw new ArgumentException (string.Format ("No part with id={0} found, aborting.", partId));

			return new List<KspObject> (new[] { part });
		}

		private static IReadOnlyList<KspObject> Parts (KspObject craft, string partName)
		{
			ConsoleWriteLineIfNotSilent (string.Format ("Searching for parts with name '{0}'...", partName));
			var occurrences = craft.FilterPartsByNamePattern (partNamePattern).ToList ();
			if (occurrences.Count <= 0)
				throw new ArgumentException (string.Format ("No parts with a name of '{0}' found, aborting.", partName));

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

		private static void ConsoleWriteIfNotSilent (string value)
		{
			if (silentExecution)
				return;

			Console.Write (value);
		}

		private static void ConsoleWriteLineIfNotSilent (string value = null)
		{
			if (silentExecution)
				return;

			Console.WriteLine (value ?? string.Empty);
		}

		private static int ParseRemoveCommand (int argIdx, params string[] args)
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

		private static int ParseListCraftsCommand (int argIdx, params string[] args)
		{
			if (command != Command.Unspecified)
				throw new ArgumentException ("");

			command = Command.ListCrafts;

			return argIdx;
		}

		private static int ParseListPartsCommand (int argIdx, params string[] args)
		{
			if (command != Command.Unspecified)
				throw new ArgumentException ("");

			command = Command.ListParts;

			return argIdx;
		}

		private static void ParseArguments (params string[] args)
		{
			for (var argIdx = 0; argIdx < args.Length; argIdx++) {
				Func<int, string[], int> parser;
				if (!ArgumentParsers.TryGetValue (args [argIdx], out parser))
					throw new ArgumentException ("Illegal argument", args [argIdx]);

				argIdx = parser (argIdx, args);
			}
		}

		private static int ParseInputFilePathArgument (int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException ("");

			using (var inputTextReader = new StreamReader (new FileStream (args [argIdx], FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8))
				inputText = inputTextReader.ReadToEnd ();

			return argIdx;
		}

		private static int ParseOutputFilePathArgument (int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException ("");

			outputTextWriter = new StreamWriter (new FileStream (args [argIdx], FileMode.Truncate, FileAccess.ReadWrite, FileShare.Read), Encoding.UTF8);
			return argIdx;
		}

//		private static int ParseCraftNameArgument (int argIdx, params string[] args)
//		{
//			argIdx++;
//			if (args.Length <= argIdx)
//				throw new ArgumentException ("");
//			
//			craftNamePattern = args [argIdx];
//			return argIdx;
//		}

		private static int ParseSilentExecutionArgument (int argIdx, params string[] args)
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
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KSPPartRemover.Backend;

namespace KSPPartRemover
{
	public static class Program
	{
		private enum Command
		{
			Unspecified,
			Remove
		}

		private static Command command;
		private static string partToRemove;
		private static TextReader inputTextReader;
		private static TextWriter outputTextWriter;
		private static bool silentExecution;

		private static readonly Dictionary<string, Func<int, string[], int>> ArgumentParsers =
			new Dictionary<string, Func<int, string[], int>>
			{
				{"-r", ParseRemoveCommand},
				{"--remove", ParseRemoveCommand},
				{"-i", ParseInputFilePathArgument},
				{"-o", ParseOutputFilePathArgument},
				{"-s", ParseSilentExecutionArgument},
				{"--silent", ParseSilentExecutionArgument}
			};

		private static readonly Dictionary<Command, Func<int>> CommandDelegates =
			new Dictionary<Command, Func<int>>
			{
				{Command.Remove, PerformRemoveCommand}
			};

		private static void PrintInfoHeader()
		{
			var assemblyName = typeof (Program).Assembly.GetName();

			var infoHeader = new StringBuilder();
			infoHeader.AppendFormat("{0} v{1}", assemblyName.Name, assemblyName.Version);
			infoHeader.AppendLine();
			infoHeader.AppendLine();
			infoHeader.Append("Compatible with KSP version: 0.24.2");

			Console.WriteLine();
			Console.WriteLine(infoHeader.ToString());
			Console.WriteLine();
		}

		private static void PrintUsage()
		{
			var assemblyName = typeof (Program).Assembly.GetName();

			Console.Write("usage: ");
			Console.Write(Path.GetFileName(assemblyName.CodeBase));
			Console.WriteLine(" COMMAND [-s | --silent] -i <inputFilePath> [-o <outputFilePath>]");
			Console.WriteLine();
			Console.WriteLine("Commands:");
			Console.WriteLine();
			Console.WriteLine("\t -r, --remove <nameOrIdOfPartToRemove>");
			Console.WriteLine("\t\t remove all parts with the given name - or id if the parameter is an integer");
			Console.WriteLine();
			Console.WriteLine("Arguments:");
			Console.WriteLine();
			Console.WriteLine("\t -i <FilePath>");
			Console.WriteLine("\t\t specifies the input file (Mandatory)");
			Console.WriteLine();
			Console.WriteLine("\t -o <FilePath>");
			Console.WriteLine("\t\t specifies the output file (Optional, prints to stdout if not specified)");
			Console.WriteLine();
			Console.WriteLine("\t -s, --silent");
			Console.WriteLine("\t\t do not print additional information and do not ask for removal confirmation");
			Console.WriteLine();
		}

		public static int Main(params string[] args)
		{
			try
			{
				try
				{
					SetDefaultArguments();
					ParseArguments(args);
				}
				catch (ArgumentException ex)
				{
					PrintInfoHeader();
					PrintUsage();

					OutputExceptionBrief(ex);
					return -1;
				}
				catch (FileNotFoundException ex)
				{
					PrintInfoHeader();

					OutputExceptionBrief(ex);
					return -1;
				}
				catch (Exception ex)
				{
					PrintInfoHeader();

					OutputExceptionDetailed(ex);
					return -1;
				}

				try
				{
					if (!silentExecution)
						PrintInfoHeader();

					if (command == Command.Unspecified)
						throw new ArgumentException("No command specified");

					if (inputTextReader == null)
						throw new ArgumentException("No input file specified");

					return CommandDelegates[command]();
				}
				catch (ArgumentException ex)
				{
					PrintUsage();

					OutputExceptionBrief(ex);
					return -1;
				}
				catch (Exception ex)
				{
					OutputExceptionDetailed(ex);
					return -1;
				}
			}
			finally
			{
				if (inputTextReader != null)
					inputTextReader.Dispose();

				if (outputTextWriter != Console.Out)
					outputTextWriter.Dispose();
			}
		}

		private static IReadOnlyList<Part> Parts(CraftFile craftFile, long partId)
		{
			var partFinder = new PartFinder(craftFile);

			ConsoleWriteLineIfNotSilent(string.Format("Searching for part with id={0}...", partId));
			var part = partFinder.FindPartById(partId);
			if (part == null)
				throw new ArgumentException(string.Format("No part with id={0} found, aborting.", partId));

			return new List<Part>(new[] {part});
		}

		private static IReadOnlyList<Part> Parts(CraftFile craftFile, string partName)
		{
			var partFinder = new PartFinder(craftFile);

			ConsoleWriteLineIfNotSilent(string.Format("Searching for parts with name '{0}'...", partName));
			var occurrences = partFinder.AllOccurrences(partToRemove);
			if (occurrences.Count <= 0)
				throw new ArgumentException(string.Format("No parts with a name of '{0}' found, aborting.", partName));

			return occurrences;
		}

		private static int PerformRemoveCommand()
		{
			if (String.IsNullOrEmpty(partToRemove))
				throw new ArgumentException("no part name specified");

			ConsoleWriteLineIfNotSilent("Loading craft file...");
			var craftFile = CraftFile.FromText(inputTextReader.ReadToEnd());

			long partId;
			var matchingParts = long.TryParse(partToRemove, out partId)
				? Parts(craftFile, partId)
				: Parts(craftFile, partToRemove);

			var partRemover = new SafePartRemover(craftFile);
			var partRemovalActions = matchingParts.AsParallel().Select(partRemover.PrepareRemovePart).ToArray();
			var mergedPartRemovalAction = partRemover.CombineRemovalActions(partRemovalActions);

			if (!silentExecution)
			{
				Console.WriteLine();
				Console.WriteLine("Would remove the following parts:");
				Console.WriteLine("=================================");
				PrintPartList(craftFile, mergedPartRemovalAction.PartsToBeRemoved);
				Console.WriteLine("=================================");
				if (!ConfirmPartRemoval())
				{
					Console.WriteLine("aborted.");
					return -1;
				}
			}

			ConsoleWriteIfNotSilent("Removing parts...");
			mergedPartRemovalAction.RemoveParts();
			ConsoleWriteLineIfNotSilent("done!");

			outputTextWriter.Write(craftFile.Content);
			return 0;
		}

		private static void PrintPartList(CraftFile craftFile, IEnumerable<Part> parts)
		{
			const int numberOfPartsPerPage = 50;

			var currentPartIdx = 1;
			foreach (var part in parts)
			{
				if (currentPartIdx++ % numberOfPartsPerPage == 0)
					if (!silentExecution)
					{
						Console.WriteLine();
						Console.Write("Press any key for next page...");
						Console.ReadKey();
						Console.WriteLine();
					}
				Console.WriteLine("{0} (id={1})", part.Name, craftFile.IdOfPart(part));
			}
		}

		private static bool ConfirmPartRemoval()
		{
			Console.Write("Perform the removal? (y/[N]) ");
			var answeredYes = Console.ReadKey().Key == ConsoleKey.Y;
			Console.WriteLine();

			return answeredYes;
		}

		private static void ConsoleWriteIfNotSilent(string value)
		{
			if (silentExecution)
				return;

			Console.Write(value);
		}

		private static void ConsoleWriteLineIfNotSilent(string value = null)
		{
			if (silentExecution)
				return;

			Console.WriteLine(value ?? string.Empty);
		}

		private static void SetDefaultArguments()
		{
			command = Command.Unspecified;
			partToRemove = null;
			inputTextReader = null;
			outputTextWriter = Console.Out;
			silentExecution = false;
		}

		private static void ParseArguments(params string[] args)
		{
			for (var argIdx = 0; argIdx < args.Length; argIdx++)
			{
				Func<int, string[], int> parser;
				if (!ArgumentParsers.TryGetValue(args[argIdx], out parser))
					throw new ArgumentException("Illegal argument", args[argIdx]);

				argIdx = parser(argIdx, args);
			}
		}

		private static int ParseInputFilePathArgument(int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException("");

			inputTextReader = new StreamReader(args[argIdx]);
			return argIdx;
		}

		private static int ParseOutputFilePathArgument(int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException("");

			outputTextWriter = new StreamWriter(args[argIdx]);
			return argIdx;
		}

		private static int ParseRemoveCommand(int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException("");

			if (command != Command.Unspecified)
				throw new ArgumentException("");

			command = Command.Remove;

			partToRemove = args[argIdx].Trim('\"');
			return argIdx;
		}

		private static int ParseSilentExecutionArgument(int argIdx, params string[] args)
		{
			silentExecution = true;
			return argIdx;
		}

		private static void OutputExceptionBrief(Exception exception)
		{
			var message = exception.Message;
			if (String.IsNullOrEmpty(message))
				message = "Invalid command line arguments.";

			Console.WriteLine();
			Console.Write("ERROR: ");
			Console.WriteLine(message);
			Console.WriteLine();
		}

		private static void OutputExceptionDetailed(Exception exception)
		{
			Console.WriteLine();
			Console.Write("EXCEPTION: ");
			Console.WriteLine(exception.ToString());
			Console.WriteLine();
		}
	}
}

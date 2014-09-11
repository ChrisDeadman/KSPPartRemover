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
		private static TextReader inputTextReader;
		private static TextWriter outputTextWriter;
		private static string nameOfPartToRemove;
		private static bool silentExecution;

		private static readonly Dictionary<string, Func<int, string[], int>> ArgumentParsers =
			new Dictionary<string, Func<int, string[], int>>
			{
				{"-i", ParseInputFileNameArgument},
				{"-o", ParseOutputFileNameArgument},
				{"-p", ParseNameOfPartToRemoveArgument},
				{"--part-name", ParseNameOfPartToRemoveArgument},
				{"-s", ParseSilentExecutionArgument},
				{"--silent", ParseSilentExecutionArgument}
			};

		private static string InfoHeader
		{
			get
			{
				var assemblyName = typeof (Program).Assembly.GetName();

				var infoHeader = new StringBuilder();
				infoHeader.AppendFormat("{0} v{1}", assemblyName.Name, assemblyName.Version);
				infoHeader.AppendLine();
				infoHeader.AppendLine();
				infoHeader.Append("Compatible with KSP version: 0.24.2");

				return infoHeader.ToString();
			}
		}

		private static void PrintUsage()
		{
			var assemblyName = typeof (Program).Assembly.GetName();

			Console.WriteLine(InfoHeader);
			Console.WriteLine();
			Console.Write("usage: ");
			Console.Write(Path.GetFileName(assemblyName.CodeBase));
			Console.Write(" -p | --part-name \"nameOfPartToRemove\" [-s | --silent] -i \"inputFile\" [-o \"outputFile\"]");
			Console.WriteLine();
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

					if (inputTextReader == null)
						throw new ArgumentException("No input file specified");

					if (String.IsNullOrEmpty(nameOfPartToRemove))
						throw new ArgumentException("no part name specified");
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

				try
				{
					ConsoleWriteLineIfNotSilent("Searching for parts...");

					var craftFile = CraftFile.FromText(inputTextReader.ReadToEnd());
					var partFinder = new PartFinder(craftFile);

					var occurrences = partFinder.AllOccurrences(nameOfPartToRemove);
					if (occurrences.Count <= 0)
					{
						Console.WriteLine("No parts with a name of '{0}' found, aborting.", nameOfPartToRemove);
						return -1;
					}

					var partRemover = new SafePartRemover(craftFile);
					var partRemovalActions = occurrences.AsParallel().Select(partRemover.PrepareRemovePart).ToArray();
					var mergedPartRemovalAction = partRemover.CombineRemovalActions(partRemovalActions);

					PrintPartsToBeRemoved(craftFile, mergedPartRemovalAction.PartsToBeRemoved);
					if (!ConfirmPartRemoval())
					{
						ConsoleWriteLineIfNotSilent("aborted.");
						return -1;
					}

					ConsoleWriteIfNotSilent("Removing parts...");
					mergedPartRemovalAction.RemoveParts();
					ConsoleWriteLineIfNotSilent("done!");

					outputTextWriter.Write(craftFile.Content);
					return 0;
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

		private static void PrintPartsToBeRemoved(CraftFile craftFile, IEnumerable<Part> partsToBeRemoved)
		{
			const int numberOfPartsPerPage = 100;

			if (silentExecution)
				return;

			ConsoleWriteLineIfNotSilent("Would remove the following parts:");
			ConsoleWriteLineIfNotSilent("---------------------------------");
			var currentPartIdx = 1;
			foreach (var partToBeRemoved in partsToBeRemoved)
			{
				if (currentPartIdx++ % numberOfPartsPerPage == 0)
				{
					ConsoleWriteIfNotSilent("Press any key for next page...");
					Console.ReadKey();
					ConsoleWriteLineIfNotSilent();
				}
				ConsoleWriteLineIfNotSilent(string.Format("{0} (id: {1})", partToBeRemoved.Name, craftFile.IdOfPart(partToBeRemoved)));
			}
			ConsoleWriteLineIfNotSilent("---------------------------------");
		}

		private static bool ConfirmPartRemoval()
		{
			if (silentExecution)
				return true;

			ConsoleWriteIfNotSilent("Perform the removal? ([Y]/n) ");
			var answeredYes = Console.ReadKey().Key != ConsoleKey.N;
			ConsoleWriteLineIfNotSilent();

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
			inputTextReader = null;
			outputTextWriter = Console.Out;
			nameOfPartToRemove = null;
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

		private static int ParseInputFileNameArgument(int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException("");

			inputTextReader = new StreamReader(args[argIdx]);
			return argIdx;
		}

		private static int ParseOutputFileNameArgument(int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException("");

			outputTextWriter = new StreamWriter(args[argIdx]);
			return argIdx;
		}

		private static int ParseNameOfPartToRemoveArgument(int argIdx, params string[] args)
		{
			argIdx++;
			if (args.Length <= argIdx)
				throw new ArgumentException("");

			nameOfPartToRemove = args[argIdx].Trim('\"');
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

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover
{
    public class ProgramUI
    {
        private const int ListEntriesPerPage = 20;

        private readonly Parameters parameters;

        public ProgramUI(Parameters parameters)
        {
            this.parameters = parameters;
        }

        public void DisplayCraftList(List<KspCraftObject> crafts)
        {
            PrintList("Crafts", crafts.Select(CraftObjectToString));
        }

        public void DisplayPartList(Dictionary<KspCraftObject, List<KspPartObject>> craftParts)
        {
            var listEntries = new List<String>();

            foreach (var craftEntry in craftParts) {
                listEntries.Add(CraftObjectToString(craftEntry.Key) + ":");
                listEntries.AddRange(craftEntry.Value.Select(part => "\t" + PartObjectToString(craftEntry.Key, part)));
            }

            PrintList("Parts", listEntries);
        }

        public void DisplayPartDependencyList(Dictionary<KspCraftObject, Dictionary<KspPartObject, List<KspPartLinkProperty>>> craftPartLinks)
        {
            var listEntries = new List<String>();

            foreach (var craftEntry in craftPartLinks) {
                listEntries.Add(CraftObjectToString(craftEntry.Key) + ":");

                foreach (var partEntry in craftEntry.Value) {
                    listEntries.Add("\t" + PartObjectToString(craftEntry.Key, partEntry.Key) + ":");
                    listEntries.AddRange(partEntry.Value.Select(partLink => "\t\t" + PartLinkPropertyToString(craftEntry.Key, partLink)));
                }
            }

            PrintList("Part Dependencies", listEntries);
        }

        public void DisplayUserMessage(String str)
        {
            if (!parameters.SilentExecution) {
                Console.WriteLine(str);
            }
        }

        public void DisplayUserList(String title, IEnumerable<String> list)
        {
            if (!parameters.SilentExecution) {
                PrintList(title, list);
            }
        }

        public void DisplayErrorMessage(String str)
        {
            Console.WriteLine($"ERROR: {str}");
        }

        public bool AskYesNoQuestion(String question)
        {
            if (parameters.SilentExecution) {
                return true;
            }

            Console.Write($"{question} (y/[N]) ");
            var answeredYes = Console.ReadKey().Key == ConsoleKey.Y;
            Console.WriteLine();

            return answeredYes;
        }

        public static String CraftObjectToString(KspCraftObject craft) => $"{craft.Name}";

        public static String PartObjectToString(KspCraftObject craft, KspPartObject part) => $"[{craft.IdOfChild(part)}]{part.Name}";

        public static String PartLinkPropertyToString(KspCraftObject craft, KspPartLinkProperty property)
        {
            var sb = new StringBuilder();
            sb.Append($"{PartObjectToString(craft, property.Part)}");
            sb.Append($"[{property.Name}");
            if (property.Prefix != null) {
                sb.Append($"({property.Prefix})");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private void PrintList(String title, IEnumerable<String> entries)
        {
            var header = "  " + title + "  ";
            DisplayUserMessage(new String('=', header.Length));
            DisplayUserMessage(header);
            DisplayUserMessage(new String('=', header.Length));

            var currentEntry = 0;
            foreach (var entry in entries) {
                if (++currentEntry >= ListEntriesPerPage) {
                    currentEntry = 0;
                    if (!parameters.SilentExecution) {
                        Console.Write("Press any key for next page...");
                        Console.ReadKey();
                        Console.WriteLine();
                    }
                }
                Console.WriteLine(entry);
            }
        }
    }
}

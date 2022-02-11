using System.Text;

namespace KSPPartRemover.Command
{
    public class Help
    {
        private readonly ProgramUI ui;

        public Help(ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute()
        {
            var assemblyName = typeof(Help).Assembly.GetName();

            var sb = new StringBuilder();
            sb.Append("usage: ");
            sb.Append(assemblyName.Name);
            sb.AppendLine(" <command> [<switches>] -i <input-file>");
            sb.AppendLine();
            sb.AppendLine("Commands:");
            sb.AppendLine();
            sb.AppendLine("\t list-crafts");
            sb.AppendLine("\t\t list crafts in the input file");
            sb.AppendLine();
            sb.AppendLine("\t list-parts");
            sb.AppendLine("\t\t list parts in the input file");
            sb.AppendLine();
            sb.AppendLine("\t list-partdeps");
            sb.AppendLine("\t\t same as 'list-parts' but also prints dependencies");
            sb.AppendLine("\t\t the part filter is applied on the dependencies for this command");
            sb.AppendLine();
            sb.AppendLine("\t list-mods");
            sb.AppendLine("\t\t list mods used by the parts in the input file");
            sb.AppendLine($"\t\t if you don't have a required mod installed it will be listed as UNKNOWN");
            sb.AppendLine("\t\t [KSP directory needs to be your current directory for this to work!]");
            sb.AppendLine();
            sb.AppendLine("\t remove-parts");
            sb.AppendLine("\t\t remove one or more parts from one or more crafts");
            sb.AppendLine();
            sb.AppendLine("Switches:");
            sb.AppendLine();
            sb.AppendLine("\t -i <path>");
            sb.AppendLine("\t\t specifies the input file");
            sb.AppendLine();
            sb.AppendLine("\t[Optional]");
            sb.AppendLine("\t -o <path>");
            sb.AppendLine("\t\t specifies the output file (modifies input file if not specified)");
            sb.AppendLine();
            sb.AppendLine("\t[Optional]");
            sb.AppendLine("\t -c, --craft <name-pattern>");
            sb.AppendLine("\t\t apply craft filter (applies to all crafts if not specified)");
            sb.AppendLine("\t\t '!' in front of the regex performs inverse matching");
            sb.AppendLine("\t\t example for name pattern: --craft \'^Asteroid\'");
            sb.AppendLine("\t\t example for inverse matching: --craft \'!Mün\'");
            sb.AppendLine();
            sb.AppendLine("\t[Optional]");
            sb.AppendLine("\t -p, --part <id or name-pattern>");
            sb.AppendLine("\t\t apply part filter (applies to all parts if not specified)");
            sb.AppendLine("\t\t if part is a number it is interpreted as the part id");
            sb.AppendLine("\t\t '!' in front of the regex performs inverse matching");
            sb.AppendLine("\t\t example for id: --part 1");
            sb.AppendLine("\t\t example for name pattern: --part \'fuelTank.*\'");
            sb.AppendLine("\t\t example for inverse matching: --part \'!^PotatoRoid$\'");
            sb.AppendLine();
            sb.AppendLine("\t[Optional]");
            sb.AppendLine("\t -s, --silent");
            sb.AppendLine("\t\t do not print additional info, do not ask for confirmation");
            sb.AppendLine();

            ui.DisplayUserMessage(sb.ToString());

            return 0;
        }
    }
}

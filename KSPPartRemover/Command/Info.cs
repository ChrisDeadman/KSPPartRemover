using System.Text;
using System.Linq;
using System.Reflection;

namespace KSPPartRemover.Command
{
    public class Info
    {
        private readonly ProgramUI ui;

        public Info(ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute()
        {
            var assembly = typeof(Info).Assembly;
            var assemblyName = assembly.GetName();
            var kspVersion = assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .First(attr => attr.Key == "KSPVersion")
                .Value;

            var sb = new StringBuilder();
            sb.AppendLine($"{assemblyName.Name} v{assemblyName.Version.Major}.{assemblyName.Version.Minor}");
            sb.AppendLine($"Compatible with KSP version: {kspVersion}");

            ui.DisplayUserMessage(sb.ToString());

            return 0;
        }
    }
}

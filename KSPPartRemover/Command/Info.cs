using System;
using System.IO;
using System.Text;

namespace KSPPartRemover.Command
{
    public class Info
    {
        private readonly ProgramUI ui;

        public Info (ProgramUI ui)
        {
            this.ui = ui;
        }

        public int Execute ()
        {
            var assemblyName = typeof(Info).Assembly.GetName ();

            var sb = new StringBuilder ();
            sb.AppendLine ($"{assemblyName.Name} v{assemblyName.Version}");
            sb.AppendLine ("Compatible with KSP version: 1.1");

            ui.DisplayUserMessage (sb.ToString ());

            return 0;
        }
    }
}

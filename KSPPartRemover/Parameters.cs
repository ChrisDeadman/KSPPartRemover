using System;
using System.IO;
using KSPPartRemover.Feature;

namespace KSPPartRemover
{
    public class Parameters
    {
        public Func<int> Command { get; set; }

        public String InputFilePath { get; set; }

        public String OutputFilePath { get; set; }

        public RegexFilter CraftFilter { get; set; }

        public RegexFilter PartFilter { get; set; }

        public bool SilentExecution { get; set; }
    }
}

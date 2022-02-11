namespace KSPPartRemover.Feature
{
    public class PartInfo
    {
        public string Name { get; }

        public string ModName { get; }

        public PartInfo(string name, string modName)
        {
            this.Name = name;
            this.ModName = modName;
        }
    }
}

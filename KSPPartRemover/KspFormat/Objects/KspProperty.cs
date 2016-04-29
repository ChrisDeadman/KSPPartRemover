using System;

namespace KSPPartRemover.KspFormat.Objects
{
    public abstract class KspProperty
    {
        public String Name { get; }

        public KspProperty (String name)
        {
            this.Name = name;
        }
    }
}

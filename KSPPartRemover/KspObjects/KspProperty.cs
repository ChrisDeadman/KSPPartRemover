using System;

namespace KSPPartRemover.KspObjects
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

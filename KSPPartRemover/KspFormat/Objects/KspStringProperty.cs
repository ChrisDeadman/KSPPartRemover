using System;

namespace KSPPartRemover.KspFormat.Objects
{
    public class KspStringProperty : KspProperty
    {
        public String Text { get; }

        public KspStringProperty(String name, String value) : base(name)
        {
            this.Text = value;
        }
    }
}

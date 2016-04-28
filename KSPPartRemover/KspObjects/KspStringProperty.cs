using System;

namespace KSPPartRemover.KspObjects
{
    public class KspStringProperty : KspProperty
    {
        public String Text { get; }

        public KspStringProperty (String name, String value) : base (name)
        {
            this.Text = value;
        }
    }
}

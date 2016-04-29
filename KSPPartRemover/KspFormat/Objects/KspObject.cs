using System;
using System.Collections.Generic;

namespace KSPPartRemover.KspFormat.Objects
{
    public class KspObject
    {
        public String Type { get; }

        public bool IsGlobalObject { get; }

        public IReadOnlyList<KspProperty> Properties { get; } = new List<KspProperty> ();

        public IReadOnlyList<KspObject> Children { get; } = new List<KspObject> ();

        public KspObject (String type, bool isGlobalObject = false)
        {
            this.IsGlobalObject = isGlobalObject;
            this.Type = type;
        }

        public KspObject Parent { get; internal set; }

        public KspObject AddProperty (KspProperty property)
        {
            ((List<KspProperty>)Properties).Add (property);
            return this;
        }

        public KspObject InsertProperty (int index, KspProperty property)
        {
            ((List<KspProperty>)Properties).Insert (Math.Min (index, Properties.Count), property);
            return this;
        }

        public KspObject RemoveProperty (KspProperty property)
        {
            ((List<KspProperty>)Properties).Remove (property);
            return this;
        }

        public KspObject AddChild (KspObject child)
        {
            child.Parent = this;
            ((List<KspObject>)Children).Add (child);
            return this;
        }

        public KspObject RemoveChild (KspObject child)
        {
            ((List<KspObject>)Children).Remove (child);
            return this;
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.KspObjects;
using KSPPartRemover.Extension;

namespace KSPPartRemover.KspObjects.Format
{
    class KspProtoObject
    {
        public KspObject Obj { get; }

        public List<String> Properties { get; }

        public List<KspProtoObject> Children { get; }

        public KspProtoObject (KspObject obj, List<String> properties, List<KspProtoObject> children)
        {
            this.Obj = obj;
            this.Properties = properties;
            this.Children = children;
        }

        public void ResolveChildren ()
        {
            foreach (var child in Children) {
                Obj.AddChild (child.Obj);
                child.ResolveChildren ();
            }
        }

        public void ResolveProperties<TProperty> () where TProperty : KspProperty
        {
            foreach (var prop in Properties) {
                var property = KspObjectReader.ReadProperty<TProperty> (Obj, prop);
                if (property != null) {
                    Obj.InsertProperty (Properties.IndexOf (prop), property);
                }
            }

            foreach (var child in Children) {
                child.ResolveProperties<TProperty> ();
            }
        }
    }
}

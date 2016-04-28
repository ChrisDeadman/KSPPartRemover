using System;
using System.Linq;
using System.Collections.Generic;
using KSPPartRemover.Extension;

namespace KSPPartRemover.KspObjects
{
    public class KspPartObject : KspObject
    {
        public const String TypeId = "PART";

        public KspPartObject (bool isGlobalObject = false) : base (TypeId, isGlobalObject)
        {
        }

        public String Name { get { return (this.Properties<KspStringProperty> ("part").FirstOrDefault () ?? this.Properties<KspStringProperty> ("name").FirstOrDefault ()).Text; } }

        public IReadOnlyList<KspPartLinkProperty> LinkRefs {
            get {
                return this.Properties<KspPartLinkProperty> ("link").ToList ();
            }
            set {
                UpdateReferences ("link", value.Where (p => p.Name == "link"));
            }
        }

        public IReadOnlyList<KspPartLinkProperty> ParentRefs {
            get {
                return this.Properties<KspPartLinkProperty> ("parent").ToList ();
            }
            set {
                UpdateReferences ("parent", value.Where (p => p.Name == "parent"));
            }
        }

        public IReadOnlyList<KspPartLinkProperty> SymRefs {
            get {
                return this.Properties<KspPartLinkProperty> ("sym").ToList ();
            }
            set {
                UpdateReferences ("sym", value.Where (p => p.Name == "sym"));
            }
        }

        public IReadOnlyList<KspPartLinkProperty> SrfNRefs {
            get {
                return this.Properties<KspPartLinkProperty> ("srfN").ToList ();
            }
            set {
                UpdateReferences ("srfN", value.Where (p => p.Name == "srfN"));
            }
        }

        public IReadOnlyList<KspPartLinkProperty> AttNRefs {
            get {
                return this.Properties<KspPartLinkProperty> ("attN").ToList ();
            }
            set {
                UpdateReferences ("attN", value.Where (p => p.Name == "attN"));
            }
        }

        private void UpdateReferences (String propertyName, IEnumerable<KspPartLinkProperty> references)
        {
            var existingProperties = this.Properties<KspPartLinkProperty> (propertyName).ToArray ();
            var insertindex = (existingProperties.Length > 0) ? Properties.ToList ().IndexOf (existingProperties.First ()) : Properties.Count;

            foreach (var property in existingProperties) {
                RemoveProperty (property);
            }

            foreach (var reference in references) {
                InsertProperty (insertindex++, reference);
            }
        }
    }
}

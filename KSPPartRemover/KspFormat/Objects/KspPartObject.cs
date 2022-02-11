using System;
using System.Linq;
using System.Collections.Generic;

namespace KSPPartRemover.KspFormat.Objects
{
    public class KspPartObject : KspObject
    {
        public const String TypeId = "PART";

        public KspPartObject(bool isGlobalObject = false) : base(TypeId, isGlobalObject)
        {
        }

        public String Name { get { return this.FirstProperty<KspStringProperty>("part", "name").Text.Split("//")[0].Trim(); } }

        public IReadOnlyList<KspPartLinkProperty> PartLinks(string propertyName)
        {
            return this.Properties<KspPartLinkProperty>(propertyName).ToList();
        }

        public void UpdatePartLinks(String propertyName, IEnumerable<KspPartLinkProperty> references)
        {
            var existingProperties = this.Properties<KspPartLinkProperty>(propertyName).ToArray();
            var insertindex = (existingProperties.Length > 0) ? Properties.ToList().IndexOf(existingProperties.First()) : Properties.Count;

            foreach (var property in existingProperties) {
                RemoveProperty(property);
            }

            foreach (var reference in references) {
                InsertProperty(insertindex++, reference);
            }
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;

namespace KSPPartRemover.KspFormat.Objects
{
    public static class KspObjectSearchExtensions
    {
        public static IEnumerable<TProp> Properties<TProp> (this KspObject obj, String name) where TProp : KspProperty =>
            obj.Properties.Where (property => property.Name.Equals (name)).OfType<TProp> ();
        
        public static TProp FirstProperty<TProp> (this KspObject obj, params String[] names) where TProp : KspProperty
        {
            foreach (var name in names) {
                var prop = obj.Properties<TProp>(name);
                if (prop.Any()) {
                    return prop.First();
                }
            }

            throw new KeyNotFoundException($"No property found matching any of the following: {String.Join(",", names)}");
        }


        public static IEnumerable<TChild> Children<TChild> (this KspObject obj, bool recursive = false) where TChild: KspObject
        {
            foreach (var child in obj.Children) {
                var match = child as TChild;
                if (match != null) {
                    yield return match;
                }

                if (recursive) {
                    var childResults = child.Children<TChild> (recursive);
                    foreach (var childResult in childResults) {
                        yield return childResult;
                    }
                }
            }
        }

        public static TChild Child<TChild> (this KspObject obj, int id) where TChild : KspObject
        {
            if (id < 0) {
                return null;
            }

            var children = obj.Children<TChild> ().ToList ();
            return (children.Count > id) ? children [id] : null;
        }

        public static int IdOfChild<TChild> (this KspObject obj, TChild child) where TChild : KspObject
        {
            var children = obj.Children<TChild> ().ToList ();
            return children.IndexOf (child);
        }
    }
}

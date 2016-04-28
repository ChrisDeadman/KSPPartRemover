using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using KSPPartRemover.KspObjects;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Tests.Extension
{
    public class KspObjectSearchExtensionsTest
    {
        [Test]
        public void CanFindPropertiesOfAnObjectByName ()
        {
            // given
            var obj = new KspObject ("OBJ")
                .AddProperty (new KspStringProperty ("property", "value1"))
                .AddProperty (new KspStringProperty ("anotherProperty", "value2"))
                .AddProperty (new KspStringProperty ("property", "value3"));
            
            // when / then
            Assert.That (obj.Properties<KspProperty> ("property"), Is.EqualTo (new[] {
                obj.Properties [0],
                obj.Properties [2]
            }));
        }

        [Test]
        public void CanFindChildrenOfAnObjectByType ()
        {
            // given
            var obj = new KspObject ("OBJ")
                .AddChild (new KspPartObject () // found
                    .AddChild (new KspPartObject ())) // not found
                .AddChild (new KspObject (KspPartObject.TypeId) // not found
                    .AddChild (new KspPartObject ())) // not found
                .AddChild (new KspPartObject () // found
                    .AddChild (new KspObject (KspPartObject.TypeId))); // not found
            
            // when / then
            Assert.That (obj.Children<KspPartObject> (), Is.EqualTo (new[] { obj.Children [0], obj.Children [2] }));
        }

        [Test]
        public void CanRecursivelyFindChildrenByType ()
        {
            // given
            var obj = new KspObject ("OBJ")
                .AddChild (new KspPartObject () // found
                    .AddChild (new KspPartObject ())) // found
                .AddChild (new KspObject (KspPartObject.TypeId) // not found
                    .AddChild (new KspPartObject ())) // found
                .AddChild (new KspPartObject () // found
                    .AddChild (new KspObject (KspPartObject.TypeId))); // not found

            // when / then
            Assert.That (obj.Children<KspPartObject> (recursive: true), Is.EqualTo (new[] {
                obj.Children [0],
                obj.Children [0].Children [0],
                obj.Children [1].Children [0],
                obj.Children [2]
            }));
        }

        [Test]
        public void CanFindChildById ()
        {
            // given
            var obj = new KspObject ("OBJ")
                .AddChild (new KspCraftObject ())
                .AddChild (new KspPartObject ())
                .AddChild (new KspPartObject ())
                .AddChild (new KspPartObject ())
                .AddChild (new KspCraftObject ());

            // when / then
            Assert.That (obj.Child<KspCraftObject> (-1), Is.Null);
            Assert.That (obj.Child<KspCraftObject> (0), Is.EqualTo (obj.Children [0])); // Ids are per object-type
            Assert.That (obj.Child<KspCraftObject> (1), Is.EqualTo (obj.Children [4]));
            Assert.That (obj.Child<KspCraftObject> (2), Is.Null);

            Assert.That (obj.Child<KspPartObject> (0), Is.EqualTo (obj.Children [1]));
            Assert.That (obj.Child<KspPartObject> (1), Is.EqualTo (obj.Children [2]));
            Assert.That (obj.Child<KspPartObject> (2), Is.EqualTo (obj.Children [3]));
        }

        [Test]
        public void CanRetrieveIdOfChild ()
        {
            // given
            var obj = new KspObject ("OBJ")
                .AddChild (new KspCraftObject ())
                .AddChild (new KspPartObject ())
                .AddChild (new KspPartObject ())
                .AddChild (new KspPartObject ())
                .AddChild (new KspCraftObject ());

            // when / then
            Assert.That (obj.IdOfChild (obj.Children [0] as KspCraftObject), Is.EqualTo (0)); // Ids are per object-type
            Assert.That (obj.IdOfChild (obj.Children [4] as KspCraftObject), Is.EqualTo (1));

            Assert.That (obj.IdOfChild (obj.Children [1] as KspPartObject), Is.EqualTo (0));
            Assert.That (obj.IdOfChild (obj.Children [2] as KspPartObject), Is.EqualTo (1));
            Assert.That (obj.IdOfChild (obj.Children [3] as KspPartObject), Is.EqualTo (2));
        }
    }
}

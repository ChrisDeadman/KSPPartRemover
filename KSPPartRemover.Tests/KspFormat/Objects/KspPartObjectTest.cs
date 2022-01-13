using System;
using System.Linq;
using NUnit.Framework;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Tests.KspFormat.Objects
{
    public class KspPartObjectTest
    {
        [Test]
        public void CanRetrieveNameOfPartFromPartProperty ()
        {
            // given
            var part = new KspPartObject ()
                .AddProperty (new KspStringProperty ("name", "thisIsIgnoredIfPartPropertyIsFound"))
                .AddProperty (new KspStringProperty ("part", "somePart")) as KspPartObject;

            // when / then
            Assert.That (part.Name, Is.EqualTo ("somePart"));
        }

        [Test]
        public void CanRetrieveNameOfPartFromNameProperty ()
        {
            // given
            var part = new KspPartObject ()
                .AddProperty (new KspStringProperty ("name", "somePart")) as KspPartObject;

            // when / then
            Assert.That (part.Name, Is.EqualTo ("somePart"));
        }

        [Test]
        public void CanRetrievePartReferences ()
        {
            // given
            var obj = new KspObject ("OBJ")
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part1")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part2")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part3")));

            var parts = obj.Children<KspPartObject> ().ToArray ();

            var part = obj.Children [0]
                .AddProperty (new KspStringProperty ("not-a-link", "not-a-link"))
                .AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Link, "top", parts [0], isIdReference: false))
                .AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Parent, "top", parts [0], isIdReference: false))
                .AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Parent, "top", parts [1], isIdReference: false)) as KspPartObject;
            
            // when / then
            Assert.That (part.PartLinks (KspPartLinkProperty.Types.Parent), Is.EqualTo (new[] { part.Properties [3], part.Properties [4] }));
        }

        [Test]
        public void CanUpdatePartReferences ()
        {
            // given
            var obj = new KspObject ("OBJ")
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part1")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part2")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part3")));

            var parts = obj.Children<KspPartObject> ().ToArray ();

            var part = obj.Children [0]
                .AddProperty (new KspStringProperty ("not-a-link", "not-a-link"))
                .AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Link, "top", parts [0], isIdReference: false))
                .AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Parent, "top", parts [0], isIdReference: false))
                .AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Parent, "top", parts [1], isIdReference: false)) as KspPartObject;

            var newParentLink = new KspPartLinkProperty (KspPartLinkProperty.Types.Parent, "bottom", parts [1], isIdReference: false);

            var expectedProperties = new [] {
                part.Properties [0],
                part.Properties [1],
                part.Properties [2],
                newParentLink
            };

            // when
            part.UpdatePartLinks (KspPartLinkProperty.Types.Parent, new[] { newParentLink });

            // then
            Assert.That (part.Properties, Is.EqualTo (expectedProperties));
        }
    }
}

using System;
using System.Linq;
using NUnit.Framework;
using KSPPartRemover.KspObjects;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Tests.KspObjects
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
				.AddProperty (new KspPartLinkProperty ("link", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("link", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("parent", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("parent", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("sym", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("sym", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("srfN", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("srfN", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("attN", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("attN", "top", parts [1], false)) as KspPartObject;
			
            // when / then
            Assert.That (part.LinkRefs, Is.EqualTo (new[] { part.Properties [1], part.Properties [2] }));
            Assert.That (part.ParentRefs, Is.EqualTo (new[] { part.Properties [3], part.Properties [4] }));
            Assert.That (part.SymRefs, Is.EqualTo (new[] { part.Properties [5], part.Properties [6] }));
            Assert.That (part.SrfNRefs, Is.EqualTo (new[] { part.Properties [7], part.Properties [8] }));
            Assert.That (part.AttNRefs, Is.EqualTo (new[] { part.Properties [9], part.Properties [10] }));
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
				.AddProperty (new KspPartLinkProperty ("link", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("link", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("parent", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("parent", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("sym", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("sym", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("srfN", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("srfN", "top", parts [1], false))
				.AddProperty (new KspPartLinkProperty ("attN", "top", parts [0], false))
				.AddProperty (new KspPartLinkProperty ("attN", "top", parts [1], false)) as KspPartObject;

            var newProperties = new[] {
                new KspPartLinkProperty ("link", "bottom", parts [0], false),
                new KspPartLinkProperty ("parent", "bottom", parts [1], false),
                new KspPartLinkProperty ("sym", "bottom", parts [0], false),
                new KspPartLinkProperty ("srfN", "bottom", parts [1], false),
                new KspPartLinkProperty ("attN", "bottom", parts [0], false)
            };

            // when / then
            part.LinkRefs = newProperties;
            Assert.That (part.LinkRefs, Is.EqualTo (new[] { newProperties [0] }));

            part.ParentRefs = newProperties;
            Assert.That (part.ParentRefs, Is.EqualTo (new[] { newProperties [1] }));

            part.SymRefs = newProperties;
            Assert.That (part.SymRefs, Is.EqualTo (new[] { newProperties [2] }));

            part.SrfNRefs = newProperties;
            Assert.That (part.SrfNRefs, Is.EqualTo (new[] { newProperties [3] }));

            part.AttNRefs = newProperties;
            Assert.That (part.AttNRefs, Is.EqualTo (new[] { newProperties [4] }));
        }
    }
}

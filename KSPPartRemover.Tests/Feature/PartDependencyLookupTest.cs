using System;
using System.Linq;
using NUnit.Framework;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Tests.Feature
{
    public class PartDependencyLookupTest
    {
        private static KspCraftObject createTestCraft ()
        {
            var craft = new KspCraftObject ()
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part1")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part2")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part3")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part4")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part5")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part6")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part7")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part8")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "part9"))) as KspCraftObject;

            var parts = craft.Children<KspPartObject> ().ToArray ();

            parts [1]
                .AddProperty (new KspPartLinkProperty ("parent", "", parts [2])); // parent is [2]

            parts [3]
                .AddProperty (new KspPartLinkProperty ("srfN", "srfAttach", parts [0])) // srfN to [0]
                .AddProperty (new KspPartLinkProperty ("srfN", "srfAttach", parts [1])); // srfN to [1]

            parts [4]
                .AddProperty (new KspPartLinkProperty ("attN", "top", null)) // attN to nirvana
                .AddProperty (new KspPartLinkProperty ("attN", "bottom", parts [3])); // attN to [3]

            parts [5]
                .AddProperty (new KspPartLinkProperty ("sym", "bottom", parts [1])); // sym to [1]

            parts [6]
                .AddProperty (new KspPartLinkProperty ("link", "bottom", parts [0])); // link to [0]

            parts [7]
                .AddProperty (new KspPartLinkProperty ("srfN", "srfAttach", parts [6])) // srfN to [6]
                .AddProperty (new KspPartLinkProperty ("srfN", "srfAttach", parts [0])); // srfN to [0]

            parts [8]
                .AddProperty (new KspPartLinkProperty ("attN", "top", parts [0])) // attN to [0]
                .AddProperty (new KspPartLinkProperty ("attN", "bottom", parts [7])) // attN to [7]
                .AddProperty (new KspPartLinkProperty ("link", "bottom", parts [1])); // link to [1]

            return craft;
        }

        [Test]
        public void CanEvaluateSoftDependenciesOnAGivenPart ()
        {
            // given
            var craft = createTestCraft ();
            var parts = craft.Children<KspPartObject> ().ToArray ();

            var dependency = parts [0];
            var expectedDependencies = new[] {
                parts [3], // srfN to [0]
                parts [6], // link to [0]
                parts [7], // srfN to [0]
                parts [8], // attN to [0]
            };

            var target = new PartLookup (craft);

            // when
            var actualDependencies = target.LookupSoftDependencies (dependency);

            // then
            Assert.That (actualDependencies, Is.EqualTo (expectedDependencies));
        }

        [Test]
        public void CanEvaluateHardDependenciesOnAGivenPart ()
        {
            // given
            var craft = createTestCraft ();
            var parts = craft.Children<KspPartObject> ().ToArray ();

            var dependency = parts [2];
            var expectedDependencies = new[] {
                parts [1], // parent is [2]
                parts [5], // sym to [1]
                // parts [8] links do not not matter for hard-dependency check
            };

            var target = new PartLookup (craft);

            // when
            var actualDependencies = target.LookupHardDependencies (dependency);

            // then
            Assert.That (actualDependencies, Is.EqualTo (expectedDependencies));
        }
    }
}

using System.Linq;
using NUnit.Framework;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Tests.Feature
{
    public class PartLookupTest
    {
        private static KspCraftObject createTestCraft()
        {
            var craft = new KspCraftObject()
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part1")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part2")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part3")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part4")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part5")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part6")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part7")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part8")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part9"))) as KspCraftObject;

            var parts = craft.Children<KspPartObject>().ToArray();

            parts[1]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Parent, "", parts[2])); // parent is [2]

            parts[3]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[0])) // srfN to [0]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[1])); // srfN to [1]

            parts[4]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "top", null)) // attN to nirvana
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "bottom", parts[3])); // attN to [3]

            parts[5]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Sym, "bottom", parts[1])); // sym to [1]

            parts[6]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "bottom", parts[0])); // link to [0]

            parts[7]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[6])) // srfN to [6]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[0])); // srfN to [0]

            parts[8]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "top", parts[0])) // attN to [0]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "bottom", parts[7])) // attN to [7]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "bottom", parts[1])); // link to [1]

            return craft;
        }

        [Test]
        public void CanLookupPartsUsingTheProvidedFilter()
        {
            // given
            var craft = createTestCraft();
            var parts = craft.Children<KspPartObject>().ToArray();
            var target = new PartLookup(craft);

            // when / then
            Assert.That(target.LookupParts(new RegexFilter("2")), Is.EqualTo(new[] { parts[2] }));
            Assert.That(target.LookupParts(new RegexFilter("part[5-7]")), Is.EqualTo(new[] { parts[4], parts[5], parts[6] }));
        }

        [Test]
        public void CanLookupSoftDependenciesOnAGivenPart()
        {
            // given
            var craft = createTestCraft();
            var parts = craft.Children<KspPartObject>().ToArray();

            var dependency = parts[0];
            var expectedDependencies = new[] {
                parts [3], // srfN to [0]
                parts [6], // link to [0]
                parts [7], // srfN to [0]
                parts [8], // attN to [0]
            };

            var target = new PartLookup(craft);

            // when
            var actualDependencies = target.LookupSoftDependencies(dependency);

            // then
            Assert.That(actualDependencies, Is.EqualTo(expectedDependencies));
        }

        [Test]
        public void CanLookupHardDependenciesOnAGivenPart()
        {
            // given
            var craft = createTestCraft();
            var parts = craft.Children<KspPartObject>().ToArray();

            var dependency = parts[2];
            var expectedDependencies = new[] {
                parts [1], // parent is [2]
                parts [5], // sym to [1]
                // parts [8] links do not not matter for hard-dependency check
            };

            var target = new PartLookup(craft);

            // when
            var actualDependencies = target.LookupHardDependencies(dependency);

            // then
            Assert.That(actualDependencies, Is.EqualTo(expectedDependencies));
        }
    }
}

using System;
using System.Linq;
using NUnit.Framework;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Tests.Feature
{
    public class CraftLookupTest
    {
        private static KspObject createTestCraft ()
        {
            return new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "craft1"));
        }

        private static KspObject createTestCraftTree ()
        {
            return new KspObject ("tree")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft1")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft2")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft3")));
        }

        [Test]
        public void CanLookupCraftFromASingleCraftObjectUsingTheProvidedFilter ()
        {
            // given
            var kspObject = createTestCraft ();
            var target = new CraftLookup (kspObject);

            // when / then
            Assert.That (target.LookupCrafts (new RegexFilter ("")).Select (craft => craft.Name), Is.EqualTo (new[] { "craft1" }));
            Assert.That (target.LookupCrafts (new RegexFilter ("craft1")).Select (craft => craft.Name), Is.EqualTo (new[] { "craft1" }));
            Assert.That (target.LookupCrafts (new RegexFilter ("craft2")), Is.Empty);
        }

        [Test]
        public void CanLookupCraftsFromACraftTreeUsingTheProvidedFilter ()
        {
            // given
            var kspObjTree = createTestCraftTree ();
            var target = new CraftLookup (kspObjTree);

            // when / then
            Assert.That (target.LookupCrafts (new RegexFilter ("2")).Select (craft => craft.Name), Is.EqualTo (new[] { "craft2" }));
            Assert.That (target.LookupCrafts (new RegexFilter ("craft[1-3]")).Select (craft => craft.Name), Is.EqualTo (new[] { "craft1", "craft2", "craft3" }));
        }
    }
}

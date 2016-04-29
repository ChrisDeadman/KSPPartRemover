using System;
using NUnit.Framework;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Tests.KspFormat.Objects
{
    public class KspCraftObjectTest
    {
        [Test]
        public void CanRetrieveNameOfCraftFromShipProperty ()
        {
            // given
            var craft = new KspCraftObject ()
                .AddProperty (new KspStringProperty ("name", "thisIsIgnoredIfShipPropertyIsFound"))
                .AddProperty (new KspStringProperty ("ship", "someCraft")) as KspCraftObject;

            // when / then
            Assert.That (craft.Name, Is.EqualTo ("someCraft"));
        }

        [Test]
        public void CanRetrieveNameOfCraftFromNameProperty ()
        {
            // given
            var craft = new KspCraftObject ()
                .AddProperty (new KspStringProperty ("name", "someCraft")) as KspCraftObject;

            // when / then
            Assert.That (craft.Name, Is.EqualTo ("someCraft"));
        }
    }
}

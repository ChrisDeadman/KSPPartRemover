using System;
using NUnit.Framework;
using KSPPartRemover.KspObjects;

namespace KSPPartRemover.Tests.KspObjects
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

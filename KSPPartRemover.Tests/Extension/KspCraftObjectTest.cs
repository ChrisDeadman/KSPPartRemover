using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Extension;
using NUnit.Framework;
using gen = KSPPartRemover.Tests.TestHelpers.KspObjectGenerator;

namespace KSPPartRemover.Tests.Extension
{
	public class KspCraftObjectTest
	{
		[Test]
		public void CanRetrieveNameOfCraftFromShipProperty ()
		{
			// given
			var craft = KspCraftObject.From (
				            gen.Craft (gen.Properties (
					            gen.Property ("name", "thisIsIgnoredIfShipPropertyIsFound"),
					            gen.Property ("ship", "someCraft")
				            )));

			// when / then
			Assert.That (craft.Name.Value, Is.EqualTo ("someCraft"));
		}

		[Test]
		public void CanRetrieveNameOfCraftFromNameProperty ()
		{
			// given
			var craft = KspCraftObject.From (
				            gen.Craft (gen.Properties (
					            gen.Property ("name", "someCraft")
				            )));

			// when / then
			Assert.That (craft.Name.Value, Is.EqualTo ("someCraft"));
		}

		[Test]
		public void CanRetrieveAllPartsOfACraft ()
		{
			// given
			var craft = KspCraftObject.From (
				            gen.Craft (gen.Properties (gen.Property ("name", "someCraft")),
					            gen.Object ("MODULE", gen.Properties ()),
					            gen.Part (gen.Properties (gen.Property ("name", "part1"))),
					            gen.Part (gen.Properties (gen.Property ("name", "part2"))),
					            gen.Part (gen.Properties (gen.Property ("name", "part3"))),
					            gen.Object ("KERBAL", gen.Properties ())));
			
			var expectedParts = new[] {
				KspPartObject.From (craft.kspObject.children [1]),
				KspPartObject.From (craft.kspObject.children [2]),
				KspPartObject.From (craft.kspObject.children [3])
			};

			// when / then
			Assert.That (craft.Parts.Value, Is.EquivalentTo (expectedParts));
		}

		[Test]
		public void CanRetrievePartByPartId ()
		{
			// given
			var craft = KspCraftObject.From (
				            gen.Craft (gen.Properties (),
					            gen.Object ("MODULE", gen.Properties (gen.Property ("name", "not a part"))),
					            gen.Part (gen.Properties (gen.Property ("name", "part1"))),
					            gen.Part (gen.Properties (gen.Property ("name", "part2"))),
					            gen.Part (gen.Properties (gen.Property ("name", "part3"))),
					            gen.Object ("KERBAL", gen.Properties (gen.Property ("name", "also not a part")))));

			// when / then
			Assert.That (craft.GetPartById (0), Is.EqualTo (KspPartObject.From (craft.kspObject.children [1])));
			Assert.That (craft.GetPartById (1), Is.EqualTo (KspPartObject.From (craft.kspObject.children [2])));
			Assert.That (craft.GetPartById (2), Is.EqualTo (KspPartObject.From (craft.kspObject.children [3])));
			Assert.That (craft.GetPartById (3), Is.Null);
		}

		[Test]
		public void CanRetrieveIdOfPart ()
		{
			// given
			var craft = KspCraftObject.From (gen.Craft (gen.Properties (),
				            gen.Object ("MODULE", gen.Properties (gen.Property ("name", "not a part"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part1"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part2"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part3"))),
				            gen.Object ("KERBAL", gen.Properties (gen.Property ("name", "also not a part")))));

			// when / then
			Assert.That (craft.GetIdOfPart (craft.Parts.Value [0]), Is.EqualTo (0));
			Assert.That (craft.GetIdOfPart (craft.Parts.Value [1]), Is.EqualTo (1));
			Assert.That (craft.GetIdOfPart (craft.Parts.Value [2]), Is.EqualTo (2));
		}
	}
}

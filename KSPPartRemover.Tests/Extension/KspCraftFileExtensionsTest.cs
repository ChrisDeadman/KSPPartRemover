using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using KSPPartRemover.Format;
using KSPPartRemover.Extension;
using gen = KSPPartRemover.Tests.TestHelpers.KspObjectGenerator;

namespace KSPPartRemover.Tests.Extension
{
	public class KspCraftFileExtensionsTest
	{
		[Test]
		public void CanRetrieveAllCraftsOfAnObject ()
		{
			// given
			var root = gen.Object ("GAME", gen.Properties (),
				           gen.Object ("MODULE", gen.Properties ()),
				           gen.Craft (gen.Properties (gen.Property ("name", "craft1"))),
				           gen.Craft (gen.Properties (gen.Property ("name", "craft2"))),
				           gen.Craft (gen.Properties (gen.Property ("name", "craft3"))),
				           gen.Object ("KERBAL", gen.Properties (gen.Property ("name", "also not a craft"))));

			var expectedCrafts = new[] {
				KspCraftObject.From (root.children [1]),
				KspCraftObject.From (root.children [2]),
				KspCraftObject.From (root.children [3])
			};

			// when / then
			Assert.That (root.GetCrafts (), Is.EquivalentTo (expectedCrafts));
		}

		[Test]
		public void GlobalVesselShouldReturnItselfForGetCrafts ()
		{
			// given
			KspObject root = gen.GlobalCraft ();

			var expectedCrafts = new[] {
				KspCraftObject.From (root),
			};
				
			// when / then
			Assert.That (root.GetCrafts (), Is.EquivalentTo (expectedCrafts));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Extension;
using NUnit.Framework;
using gen = KSPPartRemover.Tests.TestHelpers.KspObjectGenerator;

namespace KSPPartRemover.Tests.Extension
{
	public class KspObjectSearchExtensionsTest
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

			// when / then
			Assert.That (root.GetCrafts (), Is.EquivalentTo (root.children.Skip (1).Take (3)));
		}

		[Test]
		public void GlobalVesselShouldReturnItselfForGetCrafts ()
		{
			// given
			var root = gen.GlobalCraft();

			// when / then
			Assert.That (root.GetCrafts (), Is.EquivalentTo (new[] { root }));
		}

		[Test]
		public void CanRetrieveAllPartsOfACraft ()
		{
			// given
			var craft = gen.Craft (gen.Properties (gen.Property ("name", "someCraft")),
				            gen.Object ("MODULE", gen.Properties ()),
				            gen.Part (gen.Properties (gen.Property ("name", "part1"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part2"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part3"))),
				            gen.Object ("KERBAL", gen.Properties ()));

			// when / then
			Assert.That (craft.GetParts (), Is.EquivalentTo (craft.children.Skip (1).Take (3)));
		}

		[Test]
		public void CanFilterCraftsOfAnObjectByNamePattern ()
		{
			// given
			var root = gen.Object ("GAME", gen.Properties (),
				           gen.Object ("MODULE", gen.Properties (gen.Property ("name", "not a craft"))),
				           gen.Craft (gen.Properties (gen.Property ("name", "craft1"))),
				           gen.Craft (gen.Properties (gen.Property ("name", "craft2"))),
				           gen.Craft (gen.Properties (gen.Property ("name", "craft3"))),
				           gen.Object ("KERBAL", gen.Properties (gen.Property ("name", "also not a craft"))));

			// when / then
			Assert.That (root.FilterCraftsByNamePattern (".*raft[1,3]"), Is.EquivalentTo (new[] {
				root.children [1],
				root.children [3]
			}));
		}

		[Test]
		public void CanFilterPartsOfACraftByNamePattern ()
		{
			// given
			var craft = gen.Craft (gen.Properties (),
				            gen.Object ("MODULE", gen.Properties (gen.Property ("name", "not a part"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part1"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part2"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part3"))),
				            gen.Object ("KERBAL", gen.Properties (gen.Property ("name", "also not a part"))));

			// when / then
			Assert.That (craft.FilterPartsByNamePattern (".*art[1,3]"), Is.EquivalentTo (new[] {
				craft.children [1],
				craft.children [3]
			}));
		}

		[Test]
		public void CanRetrieveNameOfCraftFromShipProperty ()
		{
			// given
			var craft = gen.Craft (gen.Properties (
				            gen.Property ("name", "thisIsIgnoredIfShipPropertyIsFound"),
				            gen.Property ("ship", "someCraft")
			            ));

			// when / then
			Assert.That (craft.GetCraftName (), Is.EqualTo ("someCraft"));
		}

		[Test]
		public void CanRetrieveNameOfCraftFromNameProperty ()
		{
			// given
			var craft = gen.Craft (gen.Properties (
				            gen.Property ("name", "someCraft")
			            ));

			// when / then
			Assert.That (craft.GetCraftName (), Is.EqualTo ("someCraft"));
		}

		[Test]
		public void CanRetrieveNameOfPartFromPartProperty ()
		{
			// given
			var part = gen.Part (gen.Properties (
				           gen.Property ("name", "thisIsIgnoredIfPartPropertyIsFound"),
				           gen.Property ("part", "somePart")
			           ));

			// when / then
			Assert.That (part.GetPartName (), Is.EqualTo ("somePart"));
		}

		[Test]
		public void CanRetrieveNameOfPartFromNameProperty ()
		{
			// given
			var part = gen.Part (gen.Properties (
				           gen.Property ("name", "somePart")
			           ));

			// when / then
			Assert.That (part.GetPartName (), Is.EqualTo ("somePart"));
		}

		[Test]
		public void CanRetrievePartByPartId ()
		{
			// given
			var craft = gen.Craft (gen.Properties (),
				            gen.Object ("MODULE", gen.Properties (gen.Property ("name", "not a part"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part1"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part2"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part3"))),
				            gen.Object ("KERBAL", gen.Properties (gen.Property ("name", "also not a part"))));

			// when / then
			Assert.That (craft.GetPartById (0), Is.EqualTo (craft.children [1]));
			Assert.That (craft.GetPartById (1), Is.EqualTo (craft.children [2]));
			Assert.That (craft.GetPartById (2), Is.EqualTo (craft.children [3]));
			Assert.That (craft.GetPartById (3), Is.Null);
		}

		[Test]
		public void CanRetrieveIdOfPart ()
		{
			// given
			var craft = gen.Craft (gen.Properties (),
				            gen.Object ("MODULE", gen.Properties (gen.Property ("name", "not a part"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part1"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part2"))),
				            gen.Part (gen.Properties (gen.Property ("name", "part3"))),
				            gen.Object ("KERBAL", gen.Properties (gen.Property ("name", "also not a part"))));

			// when / then
			Assert.That (craft.GetIdOfPart (craft.children [0]), Is.LessThan (0));
			Assert.That (craft.GetIdOfPart (craft.children [1]), Is.EqualTo (0));
			Assert.That (craft.GetIdOfPart (craft.children [2]), Is.EqualTo (1));
			Assert.That (craft.GetIdOfPart (craft.children [3]), Is.EqualTo (2));
			Assert.That (craft.GetIdOfPart (craft.children [4]), Is.LessThan (0));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Extension;
using NUnit.Framework;
using KSPPartRemover.Features;
using KSPPartRemover.Format;
using gen = KSPPartRemover.Tests.TestHelpers.KspObjectGenerator;

namespace KSPPartRemover.Tests.Features
{
	public class PartRemoverTest
	{
		[Test]
		public void ProvidesInformationAboutPartsToBeRemovedBeforeRemoval ()
		{
			// given
			var part1 = gen.Part (gen.Properties (gen.Property ("name", "part1"))); // no change
			var part2 = gen.Part (gen.Properties (gen.Property ("name", "part2"), gen.Property ("parent", "2"))); //	gets removed because parent is part3
			var part3 = gen.Part (gen.Properties (gen.Property ("name", "part3"))); // removed
			var part4 = gen.Part (gen.Properties (gen.Property ("name", "part4"), gen.Property ("srfN", "srfAttach, 0"), gen.Property ("srfN", "srfAttach, 1"))); // gets removed because attached to part2
			var part5 = gen.Part (gen.Properties (gen.Property ("name", "part5"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, part4"))); // gets removed because attached to part4
			var part6 = gen.Part (gen.Properties (gen.Property ("name", "part6"), gen.Property ("sym", "1"))); // gets removed because attached to part2
			var part7 = gen.Part (gen.Properties (gen.Property ("name", "part7"), gen.Property ("sym", "0"))); // no change
			var part8 = gen.Part (gen.Properties (gen.Property ("name", "part8"), gen.Property ("srfN", "srfAttach, 6"), gen.Property ("srfN", "srfAttach, part1"))); // first srfN id is adapted
			var part9 = gen.Part (gen.Properties (gen.Property ("name", "part9"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 7"), gen.Property ("link", "part2"), gen.Property ("link", "part7"))); // second attN id is adapted, first link reference is removed

			var partsToRemove = new [] { KspPartObject.From (part3) };

			var actualCraft = KspCraftObject.From (
				                  gen.Craft (gen.Properties (gen.Property ("name", "craft")),
					                  part1,
					                  part2,
					                  part3,
					                  part4,
					                  part5,
					                  part6,
					                  part7,
					                  part8,
					                  part9
				                  ));

			var expectedPartsToBeRemoved = new[] { part3, part2, part4, part5, part6 };

			// when
			var removalAction = PartRemover.PrepareRemove (actualCraft, partsToRemove);

			// then
			Assert.That (removalAction.partsToBeRemoved.Values.Select (part => KspObjectWriter.ToString (part.kspObject)), Is.EquivalentTo (expectedPartsToBeRemoved.Select (part => KspObjectWriter.ToString (part))));
		}

		[Test]
		public void SafelyRemovesPartsByUpdatingReferencedIds ()
		{
			// given
			var part1 = gen.Part (gen.Properties (gen.Property ("name", "part1"))); // no change
			var part2 = gen.Part (gen.Properties (gen.Property ("name", "part2"), gen.Property ("parent", "2"))); //	gets removed because parent is part3
			var part3 = gen.Part (gen.Properties (gen.Property ("name", "part3"))); // removed
			var part4 = gen.Part (gen.Properties (gen.Property ("name", "part4"), gen.Property ("srfN", "srfAttach, 0"), gen.Property ("srfN", "srfAttach, 1"))); // gets removed because attached to part2
			var part5 = gen.Part (gen.Properties (gen.Property ("name", "part5"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 3"))); // gets removed because attached to part4
			var part6 = gen.Part (gen.Properties (gen.Property ("name", "part6"), gen.Property ("sym", "1"))); // gets removed because attached to part2
			var part7 = gen.Part (gen.Properties (gen.Property ("name", "part7"), gen.Property ("sym", "0"))); // removed
			var part8 = gen.Part (gen.Properties (gen.Property ("name", "part8"), gen.Property ("srfN", "srfAttach, 6"), gen.Property ("srfN", "srfAttach, 0"))); // first srfN id is adapted
			var part9 = gen.Part (gen.Properties (gen.Property ("name", "part9"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 7"), gen.Property ("link", "part2"), gen.Property ("link", "part7"))); // second attN id is adapted, first link reference is removed

			var partsToRemove = new [] { KspPartObject.From (part3) };

			var actualCraft = KspCraftObject.From (
				                  gen.Craft (gen.Properties (gen.Property ("name", "craft")),
					                  part1,
					                  part2,
					                  part3,
					                  part4,
					                  part5,
					                  part6,
					                  part7,
					                  part8,
					                  part9
				                  ));
			
			var expectedCraft = KspCraftObject.From (
				                    gen.Craft (gen.Properties (gen.Property ("name", "craft")),
					                    part1,
					                    part7,
					                    gen.Part (gen.Properties (gen.Property ("name", "part8"), gen.Property ("srfN", "srfAttach, 1"), gen.Property ("srfN", "srfAttach, 0"))),
					                    gen.Part (gen.Properties (gen.Property ("name", "part9"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 2"), gen.Property ("link", "part7")))
				                    ));

			// when
			PartRemover.PrepareRemove (actualCraft, partsToRemove).RemoveParts ();

			// then
			Assert.That (KspObjectWriter.ToString (actualCraft.kspObject), Is.EqualTo (KspObjectWriter.ToString (expectedCraft.kspObject)));
		}
	}
}

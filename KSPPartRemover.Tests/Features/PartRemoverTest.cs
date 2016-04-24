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
			var part2 = gen.Part (gen.Properties (gen.Property ("name", "part2"), gen.Property ("parent", "2"))); //	gets removed because parent is partToRemove
			var partToRemove = gen.Part (gen.Properties (gen.Property ("name", "partToRemove"))); // removed
			var part4 = gen.Part (gen.Properties (gen.Property ("name", "part4"), gen.Property ("srfN", "srfAttach, 0"), gen.Property ("srfN", "srfAttach, 1"))); // gets removed because attached to part2
			var part5 = gen.Part (gen.Properties (gen.Property ("name", "part5"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, part4"))); // gets removed because attached to part4
			var part6 = gen.Part (gen.Properties (gen.Property ("name", "part6"), gen.Property ("sym", "1"))); // gets removed because attached to part2
			var part7 = gen.Part (gen.Properties (gen.Property ("name", "part7"), gen.Property ("sym", "0"))); // no change
			var part8 = gen.Part (gen.Properties (gen.Property ("name", "part8"), gen.Property ("srfN", "srfAttach, 6"), gen.Property ("srfN", "srfAttach, part1"))); // first srfN id is adapted
			var part9 = gen.Part (gen.Properties (gen.Property ("name", "part9"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 7"), gen.Property ("link", "part2"), gen.Property ("link", "part7"))); // second attN id is adapted, first link reference is removed

			var actualCraft = gen.Craft (gen.Properties (gen.Property ("name", "craft")), new[] {
				part1,
				part2,
				partToRemove,
				part4,
				part5,
				part6,
				part7,
				part8,
				part9
			});
			var expectedPartsToBeRemoved = new[] { partToRemove, part2, part4, part5, part6 };

			var target = new PartRemover (actualCraft);

			// when
			var removalAction = target.PrepareRemovePart (partToRemove);

			// then
			Assert.That (removalAction.PartsToBeRemoved.Select (part => KspObjectWriter.ToString (part)), Is.EquivalentTo (expectedPartsToBeRemoved.Select (part => KspObjectWriter.ToString (part))));
		}

		[Test]
		public void SafelyRemovesPartsByUpdatingReferencedIds ()
		{
			// given
			var part1 = gen.Part (gen.Properties (gen.Property ("name", "part1"))); // no change
			var part2 = gen.Part (gen.Properties (gen.Property ("name", "part2"), gen.Property ("parent", "2"))); //	gets removed because parent is partToRemove
			var partToRemove = gen.Part (gen.Properties (gen.Property ("name", "partToRemove"))); // removed
			var part4 = gen.Part (gen.Properties (gen.Property ("name", "part4"), gen.Property ("srfN", "srfAttach, 0"), gen.Property ("srfN", "srfAttach, 1"))); // gets removed because attached to part2
			var part5 = gen.Part (gen.Properties (gen.Property ("name", "part5"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 3"))); // gets removed because attached to part4
			var part6 = gen.Part (gen.Properties (gen.Property ("name", "part6"), gen.Property ("sym", "1"))); // gets removed because attached to part2
			var part7 = gen.Part (gen.Properties (gen.Property ("name", "part7"), gen.Property ("sym", "0"))); // no change
			var part8 = gen.Part (gen.Properties (gen.Property ("name", "part8"), gen.Property ("srfN", "srfAttach, 6"), gen.Property ("srfN", "srfAttach, 0"))); // first srfN id is adapted
			var part9 = gen.Part (gen.Properties (gen.Property ("name", "part9"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 7"), gen.Property ("link", "part2"), gen.Property ("link", "part7"))); // second attN id is adapted, first link reference is removed

			var expectedPart1 = part1;
			// Part expectedPart2 = null;
			// Part expectedPartToRemove = null;
			// Part expectedPart4 = null;
			// Part expectedPart5 = null;
			// Part expectedPart6 = null;
			var expectedPart7 = part7;
			var expectedPart8 = gen.Part (gen.Properties (gen.Property ("name", "part8"), gen.Property ("srfN", "srfAttach, 1"), gen.Property ("srfN", "srfAttach, 0")));
			var expectedPart9 = gen.Part (gen.Properties (gen.Property ("name", "part9"), gen.Property ("attN", "top, 0"), gen.Property ("attN", "bottom, 2"), gen.Property ("link", "part7")));

			var actualCraft = gen.Craft (gen.Properties (gen.Property ("name", "craft")), new[] {
				part1,
				part2,
				partToRemove,
				part4,
				part5,
				part6,
				part7,
				part8,
				part9
			});
			var expectedCraft = gen.Craft (gen.Properties (gen.Property ("name", "craft")), new[] {
				expectedPart1,
				expectedPart7,
				expectedPart8,
				expectedPart9
			});

			var target = new PartRemover (actualCraft);

			// when
			target.PrepareRemovePart (partToRemove).RemoveParts ();

			// then
			Assert.That (KspObjectWriter.ToString (actualCraft), Is.EqualTo (KspObjectWriter.ToString (expectedCraft)));
		}
	}
}

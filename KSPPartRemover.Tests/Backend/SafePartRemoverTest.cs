using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Backend;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Backend
{
	public class SafePartRemoverTest
	{
		private static readonly Part Part1 = new Part("part1");
		private static readonly Part Part2 = new Part("part2", Property("parent", "2")); //	gets removed because parent is partToRemove
		private static readonly Part PartToRemove = new Part("partToRemove"); // removed
		private static readonly Part Part4 = new Part("part4", Property("srfN", "srfAttach, 0"), Property("srfN", "srfAttach, 1")); // gets removed because attached to part2
		private static readonly Part Part5 = new Part("part5", Property("attN", "top, 0"), Property("attN", "bottom, 3")); // gets removed because attached to part4
		private static readonly Part Part6 = new Part("part6", Property("sym", "1")); // gets removed because attached to part2
		private static readonly Part Part7 = new Part("part7", Property("sym", "0")); // no change
		private static readonly Part Part8 = new Part("part8", Property("srfN", "srfAttach, 6"), Property("srfN", "srfAttach, 0")); // first srfN id is adapted
		private static readonly Part Part9 = new Part("part9", Property("attN", "top, 0"), Property("attN", "bottom, 7")); // second attN id is adapted

		private static readonly string CraftFileText =
			Part1.Content + Environment.NewLine +
			Part2.Content + Environment.NewLine +
			PartToRemove.Content + Environment.NewLine +
			Part4.Content + Environment.NewLine +
			Part5.Content + Environment.NewLine +
			Part6.Content + Environment.NewLine +
			Part7.Content + Environment.NewLine +
			Part8.Content + Environment.NewLine +
			Part9.Content;

		[Test]
		public void ProvidesInformationAboutPartsToBeRemovedBeforeRemoval()
		{
			// given
			var expectedPartsToBeRemoved = new[]
			{
				PartToRemove,
				Part2,
				Part4,
				Part5,
				Part6
			};

			var craftFile = CraftFile.FromText(CraftFileText);
			var target = new SafePartRemover(craftFile);

			// when
			var removalAction = target.PrepareRemovePart(PartToRemove);

			// then
			Assert.That(removalAction.PartsToBeRemoved, Is.EquivalentTo(expectedPartsToBeRemoved));
		}

		[Test]
		public void SafelyRemovesPartsByUpdatingReferencedIds()
		{
			// given
			var expectedPart1 = Part1;
			// Part expectedPart2 = null;
			// Part expectedPartToRemove = null;
			// Part expectedPart4 = null;
			// Part expectedPart5 = null;
			// Part expectedPart6 = null;
			var expectedPart7 = Part7;
			var expectedPart8 = new Part("part8", Property("srfN", "srfAttach, 1"), Property("srfN", "srfAttach, 0"));
			var expectedPart9 = new Part("part9", Property("attN", "top, 0"), Property("attN", "bottom, 2"));

			var craftFile = CraftFile.FromText(CraftFileText);
			var target = new SafePartRemover(craftFile);

			// when
			target.PrepareRemovePart(PartToRemove).RemoveParts();

			// then
			Assert.That(craftFile, Is.EquivalentTo(new[] {expectedPart1, expectedPart7, expectedPart8, expectedPart9}));
		}

		private static KeyValuePair<string, string> Property(string key, string value)
		{
			return new KeyValuePair<string, string>(key, value);
		}
	}
}

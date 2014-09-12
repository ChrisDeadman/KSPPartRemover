using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Backend;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Backend
{
	public class SafePartRemoverTest
	{
		[Test]
		public void ProvidesInformationAboutPartsToBeRemovedBeforeRemoval()
		{
			// given
			var part1 = new Part("part1");
			var part2 = new Part("part2", Property("parent", "2")); //	gets removed because parent is partToRemove
			var partToRemove = new Part("partToRemove"); // removed
			var part4 = new Part("part4", Property("srfN", "srfAttach, 0"), Property("srfN", "srfAttach, 1")); // gets removed because attached to part2
			var part5 = new Part("part5", Property("attN", "top, 0"), Property("attN", "bottom, part4")); // gets removed because attached to part4
			var part6 = new Part("part6", Property("sym", "1")); // gets removed because attached to part2
			var part7 = new Part("part7", Property("sym", "0")); // no change
			var part8 = new Part("part8", Property("srfN", "srfAttach, 6"), Property("srfN", "srfAttach, part1")); // first srfN id is adapted
			var part9 = new Part("part9", Property("attN", "top, 0"), Property("attN", "bottom, 7"), Property("link", "part2"), Property("link", "part7")); // second attN id is adapted, first link reference is removed

			var craftFileText =
				part1.Content + Environment.NewLine +
				part2.Content + Environment.NewLine +
				partToRemove.Content + Environment.NewLine +
				part4.Content + Environment.NewLine +
				part5.Content + Environment.NewLine +
				part6.Content + Environment.NewLine +
				part7.Content + Environment.NewLine +
				part8.Content + Environment.NewLine +
				part9.Content;

			var expectedPartsToBeRemoved = new[]
			{
				partToRemove,
				part2,
				part4,
				part5,
				part6
			};

			var craftFile = CraftFile.FromText(craftFileText);
			var target = new SafePartRemover(craftFile);

			// when
			var removalAction = target.PrepareRemovePart(partToRemove);

			// then
			Assert.That(removalAction.PartsToBeRemoved, Is.EquivalentTo(expectedPartsToBeRemoved));
		}

		[Test]
		public void SafelyRemovesPartsByUpdatingReferencedIds()
		{
			// given
			var part1 = new Part("part1");
			var part2 = new Part("part2", Property("parent", "2")); //	gets removed because parent is partToRemove
			var partToRemove = new Part("partToRemove"); // removed
			var part4 = new Part("part4", Property("srfN", "srfAttach, 0"), Property("srfN", "srfAttach, 1")); // gets removed because attached to part2
			var part5 = new Part("part5", Property("attN", "top, 0"), Property("attN", "bottom, 3")); // gets removed because attached to part4
			var part6 = new Part("part6", Property("sym", "1")); // gets removed because attached to part2
			var part7 = new Part("part7", Property("sym", "0")); // no change
			var part8 = new Part("part8", Property("srfN", "srfAttach, 6"), Property("srfN", "srfAttach, 0")); // first srfN id is adapted
			var part9 = new Part("part9", Property("attN", "top, 0"), Property("attN", "bottom, 7"), Property("link", "part2"), Property("link", "part7")); // second attN id is adapted, first link reference is removed

			var craftFileText =
				part1.Content + Environment.NewLine +
				part2.Content + Environment.NewLine +
				partToRemove.Content + Environment.NewLine +
				part4.Content + Environment.NewLine +
				part5.Content + Environment.NewLine +
				part6.Content + Environment.NewLine +
				part7.Content + Environment.NewLine +
				part8.Content + Environment.NewLine +
				part9.Content;

			var expectedPart1 = part1;
			// Part expectedPart2 = null;
			// Part expectedPartToRemove = null;
			// Part expectedPart4 = null;
			// Part expectedPart5 = null;
			// Part expectedPart6 = null;
			var expectedPart7 = part7;
			var expectedPart8 = new Part("part8", Property("srfN", "srfAttach, 1"), Property("srfN", "srfAttach, 0"));
			var expectedPart9 = new Part("part9", Property("attN", "top, 0"), Property("attN", "bottom, 2"), Property("link", "part7"));

			var craftFile = CraftFile.FromText(craftFileText);
			var target = new SafePartRemover(craftFile);

			// when
			target.PrepareRemovePart(partToRemove).RemoveParts();

			// then
			Assert.That(craftFile, Is.EquivalentTo(new[] {expectedPart1, expectedPart7, expectedPart8, expectedPart9}));
		}

		private static KeyValuePair<string, string> Property(string key, string value)
		{
			return new KeyValuePair<string, string>(key, value);
		}
	}
}

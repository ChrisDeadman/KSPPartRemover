using System;
using System.Collections.Generic;
using KSPPartRemover.Backend;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Backend
{
	public class PartFinderTest
	{
		[Test]
		public void ReturnsEmptyListIfNoMatchIsFound()
		{
			// given
			var somePart = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var anotherPart = new Part("anotherPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));

			var craftFileText =
				somePart.Content + Environment.NewLine +
				anotherPart.Content;

			var target = new PartFinder(CraftFile.FromText(craftFileText));

			// when
			var result = target.AllOccurrences("partToFind");

			// then
			Assert.That(result, Is.EquivalentTo(new Part[0]));
		}

		[Test]
		public void CanReturnSingleOccurrenceByName()
		{
			// given
			var somePart = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var partToFind = new Part("partToFind", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var anotherPart = new Part("anotherPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));

			var craftFileText = somePart.Content + Environment.NewLine +
								partToFind.Content + Environment.NewLine +
								anotherPart.Content;

			var target = new PartFinder(CraftFile.FromText(craftFileText));

			// when
			var result = target.AllOccurrences("partToFind");

			// then
			Assert.That(result, Is.EquivalentTo(new[] {partToFind}));
		}

		[Test]
		public void CanReturnMultipleOccurrenceByName()
		{
			// given
			var somePart = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var partToFind = new Part("partToFind", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var anotherPart = new Part("anotherPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));

			var craftFileText =
				somePart.Content + Environment.NewLine +
				partToFind.Content + Environment.NewLine +
				anotherPart.Content +
				partToFind.Content + Environment.NewLine;

			var target = new PartFinder(CraftFile.FromText(craftFileText));

			// when
			var result = target.AllOccurrences("partToFind");

			// then
			Assert.That(result, Is.EquivalentTo(new[] {partToFind, partToFind}));
		}
	}
}

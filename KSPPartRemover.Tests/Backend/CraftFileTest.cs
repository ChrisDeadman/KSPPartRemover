using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Backend;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Backend
{
	public class CraftFileTest
	{
		[Test]
		public void EnumerationThrowsExceptionIfCraftFileTextIsInvalid()
		{
			// given
			var craftFileText = "PART " + Environment.NewLine +
								"{" + Environment.NewLine +
								"		 name	 =	 partToFind" + Environment.NewLine +
								"{" + Environment.NewLine +
								" anyAttribute = anyValue" + Environment.NewLine +
								"}";

			var target = CraftFile.FromText(craftFileText);

			// when / then
			Assert.That(() => target.ToArray(), Throws.TypeOf<FormatException>());
		}

		[Test]
		public void CanReturnEnumerationOfAllParts()
		{
			// given
			var somePart = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var aDifferentPart = new Part("aDifferentPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var anotherPart = new Part("anotherPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));

			var craftFileText =
				somePart.Content + Environment.NewLine +
				aDifferentPart.Content + Environment.NewLine +
				anotherPart.Content;

			var target = CraftFile.FromText(craftFileText);

			// then
			Assert.That(target, Is.EquivalentTo(new[] {somePart, aDifferentPart, anotherPart}));
		}

		[Test]
		public void IdThrowsExceptionIfCraftFileTextIsInvalid()
		{
			// given
			var craftFileText = "PART " + Environment.NewLine +
								"{" + Environment.NewLine +
								"		 name	 =	 partToFind" + Environment.NewLine +
								"{" + Environment.NewLine +
								" anyAttribute = anyValue" + Environment.NewLine +
								"}";

			var target = CraftFile.FromText(craftFileText);

			// when / then
			Assert.That(() => target.IdOfPart(new Part("dummyName")), Throws.TypeOf<FormatException>());
		}

		[Test]
		public void IdReturnsNegativeNumberIfPartIsNotFound()
		{
			// given
			var craftFileText = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue")).Content;

			var target = CraftFile.FromText(craftFileText);

			// when / then
			Assert.That(target.IdOfPart(new Part("aDifferentPart")), Is.LessThan(0));
		}

		[Test]
		public void CanReturnIdOfPart()
		{
			// given
			var somePart = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var partToFind = new Part("partToFind", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var anotherPart = new Part("anotherPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));

			var craftFileText =
				somePart.Content + Environment.NewLine +
				partToFind.Content + Environment.NewLine +
				partToFind.Content + Environment.NewLine +
				anotherPart.Content;

			var target = CraftFile.FromText(craftFileText);

			// when
			var result = target.IdOfPart(partToFind);

			// then
			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void CanReplacePart()
		{
			// given
			var somePart = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var partToReplace = new Part("partToReplace", new KeyValuePair<string, string>("replacedAttribute", "replacedValue"));
			var anotherPart = new Part("anotherPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));

			var craftFileText =
				somePart.Content + Environment.NewLine +
				partToReplace.Content + Environment.NewLine +
				anotherPart.Content;

			var replacementPart = new Part("replacementPart", new KeyValuePair<string, string>("replacementAttribute", "replacementValue"));

			var target = CraftFile.FromText(craftFileText);

			// when
			target.ReplacePart(partToReplace, replacementPart);

			// then
			Assert.That(target, Is.EquivalentTo(new[] {somePart, replacementPart, anotherPart}));
		}

		[Test]
		public void CanRemovePart()
		{
			// given
			var somePart = new Part("somePart", new KeyValuePair<string, string>("anyAttribute", "someValue"));
			var partToRemove = new Part("partToReplace", new KeyValuePair<string, string>("replacedAttribute", "replacedValue"));
			var anotherPart = new Part("anotherPart", new KeyValuePair<string, string>("anyAttribute", "someValue"));

			var craftFileText =
				somePart.Content + Environment.NewLine +
				partToRemove.Content + Environment.NewLine +
				anotherPart.Content;

			var target = CraftFile.FromText(craftFileText);

			// when
			target.RemovePart(partToRemove);

			// then
			Assert.That(target, Is.EquivalentTo(new[] {somePart, anotherPart}));
			Assert.That(target.Content, Is.Not.StringContaining("\r\n\r\n"));
		}
	}
}

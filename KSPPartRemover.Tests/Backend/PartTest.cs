using System;
using System.Collections.Generic;
using KSPPartRemover.Backend;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Backend
{
	public class PartTest
	{
		[Test]
		public void CanCreatePart()
		{
			// given
			var expectedContent = "PART " + Environment.NewLine +
								"{" + Environment.NewLine +
								"name = somePartName" + Environment.NewLine +
								"someAttribute = someValue" + Environment.NewLine +
								"anotherAttribute = aDifferentValue" + Environment.NewLine +
								"}";

			// when
			var target = new Part(
				"somePartName",
				new KeyValuePair<string, string>("someAttribute", "someValue"),
				new KeyValuePair<string, string>("anotherAttribute", "aDifferentValue"));

			// then
			Assert.That(target.Content, Is.EqualTo(expectedContent));
		}

		[Test]
		public void NameThrowsExceptionIfContentIsInvalid()
		{
			// given
			var target = Part.FromContent(
				"PART " + Environment.NewLine +
				"{" + Environment.NewLine +
				"anyAttribute = anyValue" + Environment.NewLine +
				"}");

			// when / then
			Assert.That(() => target.Name, Throws.TypeOf<FormatException>());
		}

		[Test]
		public void CanReturnNameOfPartFromPartProperty()
		{
			// given
			var target = Part.FromContent(
				"PART " + Environment.NewLine +
				"{" + Environment.NewLine +
				"	  part 	 =		somePart	 " + Environment.NewLine +
				" name = thisIsIgnoredIfPartPropertyIsFound" + Environment.NewLine +
				" anyAttribute = someValue" + Environment.NewLine +
				"}");

			// when
			var result = target.Name;

			// then
			Assert.That(result, Is.EqualTo("somePart"));
		}

		[Test]
		public void CanReturnNameOfPartFromNameProperty()
		{
			// given
			var target = Part.FromContent(
				"PART " + Environment.NewLine +
				"{" + Environment.NewLine +
				"	  name 	 =		somePart	 " + Environment.NewLine +
				" anyAttribute = someValue" + Environment.NewLine +
				"}");

			// when
			var result = target.Name;

			// then
			Assert.That(result, Is.EqualTo("somePart"));
		}

		[Test]
		public void CanGetMultiPropertyValues()
		{
			// given
			var target = new Part(
				"AnyName",
				new KeyValuePair<string, string>("aProperty", "aValue"),
				new KeyValuePair<string, string>("\tdesiredProperty", "value1"),
				new KeyValuePair<string, string>(" desiredProperty", "value2"),
				new KeyValuePair<string, string>("anotherProperty", "anotherValue"));

			// when
			var result = target.GetMultiPropertyValues("desiredProperty");

			// then
			Assert.That(result, Is.EquivalentTo(new[] {"value1", "value2"}));
		}

		[Test]
		public void DoesNotSupportToSetNewMultiPropertyValuesAtTheMoment()
		{
			// given
			var target = new Part(
				"AnyName",
				new KeyValuePair<string, string>("aProperty", "aValue"),
				new KeyValuePair<string, string>("anotherProperty", "anotherValue"));

			// when / then
			Assert.That(() => target.SetMultiPropertyValues("newProperty", "value1", "value2"), Throws.TypeOf<NotSupportedException>());
		}

		[Test]
		public void CanReplaceMultiPropertyValues()
		{
			// given
			var target = new Part(
				"AnyName",
				new KeyValuePair<string, string>("aProperty", "aValue"),
				new KeyValuePair<string, string>("\tpropertyToReplace", "removedPropertyValue"),
				new KeyValuePair<string, string>("anotherProperty", "anotherValue"),
				new KeyValuePair<string, string>("propertyToReplace", "thisIsAlsoRemoved"));

			var expectedResult = new Part(
				"AnyName",
				new KeyValuePair<string, string>("aProperty", "aValue"),
				new KeyValuePair<string, string>("\tpropertyToReplace", "value1"),
				new KeyValuePair<string, string>("\tpropertyToReplace", "value2"),
				new KeyValuePair<string, string>("anotherProperty", "anotherValue"));

			// when
			target.SetMultiPropertyValues("propertyToReplace", "value1", "value2");

			// then
			Assert.That(target, Is.EqualTo(expectedResult));
		}

		[Test]
		public void CanRemoveMultiPropertyValues()
		{
			// given
			var target = new Part(
				"AnyName",
				new KeyValuePair<string, string>("aProperty", "aValue"),
				new KeyValuePair<string, string>("propertyToRemove", "removedPropertyValue"),
				new KeyValuePair<string, string>("anotherProperty", "anotherValue"),
				new KeyValuePair<string, string>("\tpropertyToRemove", "thisIsAlsoRemoved"));

			var expectedResult = new Part(
				"AnyName",
				new KeyValuePair<string, string>("aProperty", "aValue"),
				new KeyValuePair<string, string>("anotherProperty", "anotherValue"));

			// when
			target.RemoveMultiPropertyValues("propertyToRemove");

			// then
			Assert.That(target, Is.EqualTo(expectedResult));
		}
	}
}

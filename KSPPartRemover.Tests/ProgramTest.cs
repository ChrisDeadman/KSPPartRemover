using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KSPPartRemover.Backend;
using NUnit.Framework;

namespace KSPPartRemover.Tests
{
	public class ProgramTest
	{
		private static readonly StringBuilder StdOutput = new StringBuilder();
		private static readonly StringWriter StdOutputWriter = new StringWriter(StdOutput);

		[TestFixtureSetUp]
		public static void TestFixtureSetUp()
		{
			Console.SetOut(StdOutputWriter);
		}

		[TestFixtureTearDown]
		public static void TestFixtureTearDown()
		{
			StdOutputWriter.Dispose();
		}

		[TearDown]
		public void TearDown()
		{
			StdOutput.Clear();
		}

		[Test]
		public void PrintsUsageOnError()
		{
			// when
			Program.Main();

			// then
			Assert.That(StdOutput.ToString(), Is.StringContaining("usage: "));
		}

		[Test]
		public void PrintsErrorOnError()
		{
			// when
			Program.Main();

			// then
			Assert.That(StdOutput.ToString(), Is.StringContaining("ERROR: "));
		}

		[Test]
		public void HasReturnValueLessThanZeroIfIfArgumentsAreInvalid()
		{
			// when
			var returnCode = Program.Main();

			// then
			Assert.That(returnCode, Is.LessThan(0));
		}

		[Test]
		public void PrintsAndReturnsErrorIfPartIdToReplaceIsNotFound()
		{
			// given
			var inputCraftText =
				new Part("somePart").Content +
				new Part("anotherPart").Content;

			const string inputFileName = "input.txt";
			File.WriteAllText(inputFileName, inputCraftText);

			// when
			var returnCode = Program.Main("-r", "2", "-i", inputFileName);

			// then
			Assert.That(StdOutput.ToString(), Is.StringContaining("No part with id=2 found"));
			Assert.That(returnCode, Is.LessThan(0));
		}

		// Important: Note here that we uniquely identify parts by their content.
		// This was a design decision so a partdoes not need to depend on the whole craft file.
		// It is also no problem since in reality no two part definitions can be the same unless they are EXACT duplicates and overlap each other
		// - in which case it is good to remove them :-)
		[Test]
		public void CanReplacePartByIdAndOutputResultToStdOut()
		{
			// given
			var inputCraftText =
				new Part("partToRemove", new KeyValuePair<string, string>("someParameter", "someValue")).Content +
				new Part("somePart").Content +
				new Part("partToRemove", new KeyValuePair<string, string>("someParameter", "anotherValue")).Content;

			var expectedResult =
				new Part("somePart").Content +
				new Part("partToRemove", new KeyValuePair<string, string>("someParameter", "anotherValue")).Content;

			const string inputFileName = "input.txt";
			File.WriteAllText(inputFileName, inputCraftText);

			// when
			var returnCode = Program.Main("-r", "0", "-i", inputFileName, "-s");

			// then
			Assert.That(StdOutput.ToString(), Is.EqualTo(expectedResult));
			Assert.That(returnCode, Is.EqualTo(0));
		}

		[Test]
		public void PrintsAndReturnsErrorIfPartNameToReplaceIsNotFound()
		{
			// given
			var inputCraftText =
				new Part("somePart").Content +
				new Part("anotherPart").Content;

			const string inputFileName = "input.txt";
			File.WriteAllText(inputFileName, inputCraftText);

			// when
			var returnCode = Program.Main("-r", "nonExistingPart", "-i", inputFileName);

			// then
			Assert.That(StdOutput.ToString(), Is.StringContaining("No parts with a name of 'nonExistingPart' found"));
			Assert.That(returnCode, Is.LessThan(0));
		}

		[Test]
		public void CanReplacePartByNameAndOutputResultToStdOut()
		{
			// given
			var inputCraftText =
				new Part("partToRemove", new KeyValuePair<string, string>("someParameter", "someValue")).Content +
				new Part("somePart").Content +
				new Part("partToRemove", new KeyValuePair<string, string>("someParameter", "anotherValue")).Content;

			var expectedResult =
				new Part("somePart").Content;

			const string inputFileName = "input.txt";
			File.WriteAllText(inputFileName, inputCraftText);

			// when
			var returnCode = Program.Main("-r", "partToRemove", "-i", inputFileName, "-s");

			// then
			Assert.That(StdOutput.ToString(), Is.EqualTo(expectedResult));
			Assert.That(returnCode, Is.EqualTo(0));
		}
	}
}

using System;
using System.IO;
using System.Text;
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
		public void PrintsAndReturnsErrorIfPartToReplaceIsNotFound()
		{
			// given
			const string inputCraftText =
				"PART" +
				"{" +
				"name = somePart" +
				"}";

			const string inputFileName = "input.txt";
			File.WriteAllText(inputFileName, inputCraftText);

			// when
			var returnCode = Program.Main("--part-name", "nonExistingPart", "-i", inputFileName);

			// then
			Assert.That(StdOutput.ToString(), Is.StringContaining("No parts with a name of 'nonExistingPart' found"));
			Assert.That(returnCode, Is.LessThan(0));
		}

		[Test]
		public void CanReplaceSimplePartAndOutputResultToStdOut()
		{
			// given
			const string inputCraftText =
				"PART" +
				"{" +
				"name = somePart" +
				"}" +
				"PART" +
				"{" +
				"name = partToRemove" +
				"}";

			const string expectedOutputCraftText =
				"PART" +
				"{" +
				"name = somePart" +
				"}";

			const string inputFileName = "input.txt";
			File.WriteAllText(inputFileName, inputCraftText);

			// when
			var returnCode = Program.Main("--part-name", "partToRemove", "-i", inputFileName, "--silent");

			// then
			Assert.That(StdOutput.ToString(), Is.EqualTo(expectedOutputCraftText));
			Assert.That(returnCode, Is.EqualTo(0));
		}
	}
}

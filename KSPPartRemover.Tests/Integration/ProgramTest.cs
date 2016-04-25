using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KSPPartRemover.Extension;
using NUnit.Framework;
using gen = KSPPartRemover.Tests.TestHelpers.KspObjectGenerator;
using KSPPartRemover.Format;

namespace KSPPartRemover.Tests.Integration
{
	public class ProgramTest
	{
		private static readonly StringBuilder StdOutput = new StringBuilder ();
		private static readonly StringWriter StdOutputWriter = new StringWriter (StdOutput);

		[TestFixtureSetUp]
		public static void TestFixtureSetUp ()
		{
			Console.SetOut (StdOutputWriter);
		}

		[TestFixtureTearDown]
		public static void TestFixtureTearDown ()
		{
			StdOutputWriter.Dispose ();
		}

		[TearDown]
		public void TearDown ()
		{
			StdOutput.Clear ();
		}

		[Test]
		public void PrintsUsageOnError ()
		{
			// when
			Program.Main ();

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("usage: "));
		}

		[Test]
		public void PrintsErrorMessageOnError ()
		{
			// when
			Program.Main ();

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("ERROR: "));
		}

		[Test]
		public void HasReturnValueLessThanZeroIfArgumentsAreInvalid ()
		{
			// when
			var returnCode = Program.Main ();

			// then
			Assert.That (returnCode, Is.LessThan (0));
		}

		[Test]
		public void CanRemovePartByIdAndOutputResult ()
		{
			// given
			var inputCraft = gen.GlobalCraft (
				                 gen.Object ("NOT_A_PART", gen.Properties (gen.Property ("name", "fuelTank"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "strut"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))));

			var expectedCraft = gen.GlobalCraft (
				                    gen.Object ("NOT_A_PART", gen.Properties (gen.Property ("name", "fuelTank"))),
				                    gen.Part (gen.Properties (gen.Property ("name", "strut"))),
				                    gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))));
			
			var inputText = KspObjectWriter.ToString (inputCraft);
			var expectedResult = KspObjectWriter.ToString (expectedCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "0", "-i", "input.txt", "-s");

			// then
			Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
			Assert.That (returnCode, Is.EqualTo (0));
		}

		[Test]
		public void CanRemovePartByNameAndOutputResult ()
		{
			// given
			var inputCraft = gen.GlobalCraft (
				                 gen.Object ("NOT_A_PART", gen.Properties (gen.Property ("name", "fuelTank"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "strut"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))));

			var expectedCraft = gen.GlobalCraft (
				                    gen.Object ("NOT_A_PART", gen.Properties (gen.Property ("name", "fuelTank"))),
				                    gen.Part (gen.Properties (gen.Property ("name", "strut"))));

			var inputText = KspObjectWriter.ToString (inputCraft);
			var expectedResult = KspObjectWriter.ToString (expectedCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "fuelTank", "-i", "input.txt", "-s");

			// then
			Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
			Assert.That (returnCode, Is.EqualTo (0));
		}

		[Test]
		public void CanRemovePartsOfMultipleCraftsAndOutputResult ()
		{
			// given
			var inputCrafts = gen.Object ("GAME", gen.Properties (),
				                  gen.Craft (gen.Properties (gen.Property ("name", "craft1")),
					                  gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))),
					                  gen.Part (gen.Properties (gen.Property ("name", "strut"))),
					                  gen.Part (gen.Properties (gen.Property ("name", "fuelTank")))),
				                  gen.Craft (gen.Properties (gen.Property ("name", "craft2")),
					                  gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))),
					                  gen.Part (gen.Properties (gen.Property ("name", "strut"))),
					                  gen.Part (gen.Properties (gen.Property ("name", "fuelTank")))),
				                  gen.Craft (gen.Properties (gen.Property ("name", "craft3")),
					                  gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))),
					                  gen.Part (gen.Properties (gen.Property ("name", "strut"))),
					                  gen.Part (gen.Properties (gen.Property ("name", "fuelTank")))));

			var expectedCrafts = gen.Object ("GAME", gen.Properties (),
				                     gen.Craft (gen.Properties (gen.Property ("name", "craft1")),
					                     gen.Part (gen.Properties (gen.Property ("name", "strut")))),
				                     gen.Craft (gen.Properties (gen.Property ("name", "craft2")),
					                     gen.Part (gen.Properties (gen.Property ("name", "fuelTank"))),
					                     gen.Part (gen.Properties (gen.Property ("name", "strut"))),
					                     gen.Part (gen.Properties (gen.Property ("name", "fuelTank")))),
				                     gen.Craft (gen.Properties (gen.Property ("name", "craft3")),
					                     gen.Part (gen.Properties (gen.Property ("name", "strut")))));

			var inputText = KspObjectWriter.ToString (inputCrafts);
			var expectedResult = KspObjectWriter.ToString (expectedCrafts);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "fuelTank", "-c", ".*raft[1,3]", "-i", "input.txt", "-s");

			// then
			Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
			Assert.That (returnCode, Is.EqualTo (0));
		}

		[Test]
		public void CanPrintCraftList ()
		{
			// given
			var inputCrafts = gen.Object ("GAME", gen.Properties (),
				                  gen.Craft (gen.Properties (gen.Property ("name", "someCraft"))),
				                  gen.Craft (gen.Properties (gen.Property ("name", "anotherCraft"))),
				                  gen.Craft (gen.Properties (gen.Property ("name", "ignored"))));

			var inputText = KspObjectWriter.ToString (inputCrafts);
			var expectedResult = "someCraft" + Environment.NewLine + "anotherCraft" + Environment.NewLine;

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("list-crafts", "-c", ".*Craft", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
			Assert.That (returnCode, Is.EqualTo (0));
		}

		[Test]
		public void CanPrintPartList ()
		{
			// given
			var inputCrafts = gen.Object ("GAME", gen.Properties (),
				                  gen.Craft (gen.Properties (gen.Property ("name", "someCraft")),
					                  gen.Part (gen.Properties (gen.Property ("name", "somePart")))),
				                  gen.Craft (gen.Properties (gen.Property ("name", "anotherCraft")),
					                  gen.Part (gen.Properties (gen.Property ("name", "somePart"))),
					                  gen.Part (gen.Properties (gen.Property ("name", "anotherPart")))),
				                  gen.Craft (gen.Properties (gen.Property ("name", "ignored")),
					                  gen.Part (gen.Properties (gen.Property ("name", "somePart")))));

			var inputText = KspObjectWriter.ToString (inputCrafts);
			var expectedResult =
				"someCraft:" + Environment.NewLine +
				"\tsomePart (id=0)" + Environment.NewLine +
				"anotherCraft:" + Environment.NewLine +
				"\tsomePart (id=0)" + Environment.NewLine +
				"\tanotherPart (id=1)" + Environment.NewLine;

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("list-parts", "-c", ".*Craft", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
			Assert.That (returnCode, Is.EqualTo (0));
		}

		[Test]
		public void PrintsAndReturnsErrorIfPartIdToRemoveIsNotFound ()
		{
			// given
			var inputCraft = gen.GlobalCraft (
				                 gen.Object ("NOT_A_PART", gen.Properties (gen.Property ("name", "notAPart"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "somePart"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "anotherPart"))));

			var inputText = KspObjectWriter.ToString (inputCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "2", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("No part with id=2 found"));
			Assert.That (returnCode, Is.LessThan (0));
		}

		[Test]
		public void PrintsAndReturnsErrorIfPartNameToRemoveIsNotFound ()
		{
			// given
			var inputCraft = gen.GlobalCraft (
				                 gen.Object ("NOT_A_PART", gen.Properties (gen.Property ("name", "notAPart"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "somePart"))),
				                 gen.Part (gen.Properties (gen.Property ("name", "anotherPart"))));

			var inputText = KspObjectWriter.ToString (inputCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "nonExistingPart", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("No parts with a name of 'nonExistingPart' found"));
			Assert.That (returnCode, Is.LessThan (0));
		}

		[Test]
		public void PrintsAndReturnsErrorIfNoCraftWithMatchingCraftNameIsFound ()
		{
			// given
			var inputCrafts = gen.Object ("GAME", gen.Properties (),
				                  gen.Craft (gen.Properties (gen.Property ("name", "someCraft"))),
				                  gen.Craft (gen.Properties (gen.Property ("name", "anotherCraft"))));

			var inputText = KspObjectWriter.ToString (inputCrafts);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "somePart", "--craft", "nonExistingCraft", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("No craft matching 'nonExistingCraft' found, aborting"));
			Assert.That (returnCode, Is.LessThan (0));
		}
	}
}

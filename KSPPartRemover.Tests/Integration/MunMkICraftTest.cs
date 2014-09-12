using System;
using System.IO;
using KSPPartRemover.Tests.Properties;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Integration
{
	public class MunMkICraftTest
	{
		[Test]
		public void CanRemoveBoostersAttachedToNoseConeFromCraftFile()
		{
			// given
			const string inputFileName = "input.txt";
			const string outputFileName = "result.txt";

			var inputCraftText = Resources.Mün_Mk_I_in_craft;
			var expectedOutputCraftText = Resources.Mün_Mk_I_expected_craft;

			File.WriteAllText(inputFileName, inputCraftText);

			// when
			var returnCode = Program.Main("--remove", "noseCone_4294253786", "-i", inputFileName, "-o", outputFileName, "--silent");

			// then
			Assert.That(returnCode, Is.EqualTo(0));
			Assert.That(File.ReadAllText(outputFileName), Is.EqualTo(expectedOutputCraftText));
		}
	}
}

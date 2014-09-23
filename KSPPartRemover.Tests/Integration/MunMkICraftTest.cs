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
			const string tempFileName = "temp.txt";

			var inputCraftText = Resources.Mün_Mk_I_in_craft;
			var expectedOutputCraftText = Resources.Mün_Mk_I_expected_craft;

			File.WriteAllText(tempFileName, inputCraftText);

			// when
			var returnCode = Program.Main("--remove", "noseCone_4294253786", "-i", tempFileName, "-o", tempFileName, "--silent");

			// then
			Assert.That(returnCode, Is.EqualTo(0));
			Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
		}
	}
}

using System;
using System.IO;
using KSPPartRemover.Tests.Properties;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Integration
{
	public class SpaceStationTest
	{
		[Test]
		public void CanRemoveAllLaddersFromSpaceStation() // That's why I actually developed this thing ;-)
		{
			// given
			const string tempFileName = "temp.txt";

			var inputCraftText = Resources._5bc46e52_b7ff_47b2_b04d_c6227e47264f_in;
			var expectedOutputCraftText = Resources._5bc46e52_b7ff_47b2_b04d_c6227e47264f_expected;

			File.WriteAllText(tempFileName, inputCraftText);

			// when
			var returnCode = Program.Main("--remove", "ladder1", "-i", tempFileName, "-o", tempFileName, "--silent");

			// then
			Assert.That(returnCode, Is.EqualTo(0));
			Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
		}
	}
}

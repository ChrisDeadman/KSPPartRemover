using System;
using System.IO;
using System.Reflection;
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

			var inputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.5bc46e52-b7ff-47b2-b04d-c6227e47264f.in.txt")).ReadToEnd();
			var expectedOutputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.5bc46e52-b7ff-47b2-b04d-c6227e47264f.expected.txt")).ReadToEnd();

			File.WriteAllText(tempFileName, inputCraftText);

			// when
			var returnCode = Program.Main("--remove", "ladder1", "-i", tempFileName, "-o", tempFileName, "--silent");

			// then
			Assert.That(returnCode, Is.EqualTo(0));
			Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
		}
	}
}

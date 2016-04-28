using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Integration
{
	public class MunMkICraftTest
	{
		[Test]
		public void CanRemoveBoostersAttachedToNoseConeFromCraftFile()
		{
			// given
			const String tempFileName = "temp.txt";

			var inputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Mün Mk I.in.craft")).ReadToEnd();
			var expectedOutputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Mün Mk I.expected.craft")).ReadToEnd();

			File.WriteAllText(tempFileName, inputCraftText);

			// when
			var returnCode = Program.Main("remove-part", "noseCone_4294253786", "-i", tempFileName, "-o", tempFileName, "--silent");

			// then
			Assert.That(returnCode, Is.EqualTo(0));
			Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
		}
	}
}

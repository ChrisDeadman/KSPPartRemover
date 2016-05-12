using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Integration
{
    public class SaveFileTest
    {
        [Test]
        public void CanRemoveEnginesFromAllBowserCrafts ()
        {
            // given
            const String tempFileName = "temp.txt";

            var inputCraftText = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Refuel at Minmus.in.sfs")).ReadToEnd ();
            var expectedOutputCraftText = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Refuel at Minmus.expected.sfs")).ReadToEnd ();

            File.WriteAllText (tempFileName, inputCraftText);

            // when
            var returnCode = Program.Main ("remove-parts", "--part", "liquidEngine.*", "--craft", "Bowser.*", "-i", tempFileName, "-o", tempFileName, "--silent");

            // then
            Assert.That (returnCode, Is.EqualTo (0));
            Assert.That (File.ReadAllText (tempFileName), Is.EqualTo (expectedOutputCraftText));
        }
    }
}

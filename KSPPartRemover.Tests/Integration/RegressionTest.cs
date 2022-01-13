using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Integration
{
    public class RegressionTest
    {
        [Test]
        public void CanRemoveAllLaddersFromSpaceStation() // That's why I actually developed this thing ;-)
        {
            // given
            const String tempFileName = "temp.txt";

            var inputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.5bc46e52-b7ff-47b2-b04d-c6227e47264f.in.craft")).ReadToEnd();
            var expectedOutputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.5bc46e52-b7ff-47b2-b04d-c6227e47264f.expected.craft")).ReadToEnd();

            File.WriteAllText(tempFileName, inputCraftText);

            // when
            var returnCode = Program.Main("remove-parts", "--part", "ladder1", "-i", tempFileName, "--silent");

            // then
            Assert.That(returnCode, Is.EqualTo(0));
            Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
        }


        [Test]
        public void CanRemoveBoostersAttachedToNoseConeFromMunMk1()
        {
            // given
            const String tempFileName = "temp.txt";

            var inputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Mün Mk I.in.craft")).ReadToEnd();
            var expectedOutputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Mün Mk I.expected.craft")).ReadToEnd();

            File.WriteAllText(tempFileName, inputCraftText);

            // when
            var returnCode = Program.Main("remove-parts", "--part", "noseCone_4294253786", "-i", tempFileName, "--silent");

            // then
            Assert.That(returnCode, Is.EqualTo(0));
            Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
        }

        [Test]
        public void CanRemoveModPartsFromSuperHeavy()
        {
            // given
            const String tempFileName = "temp.txt";

            var inputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Super Heavy.in.craft")).ReadToEnd();
            var expectedOutputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Super Heavy.expected.craft")).ReadToEnd();

            File.WriteAllText(tempFileName, inputCraftText);

            // when
            var returnCode = Program.Main("remove-parts", "--part", "nflv-engine-ar1-1|nflv-rcs-aero-heavy-2", "-i", tempFileName, "--silent");

            // then
            Assert.That(returnCode, Is.EqualTo(0));
            Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
        }

        [Test]
        public void CanRemoveLaddersFromSuperHeavyLander()
        {
            // given
            const String tempFileName = "temp.txt";

            var inputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Super-Heavy Lander.in.craft")).ReadToEnd();
            var expectedOutputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Super-Heavy Lander.expected.craft")).ReadToEnd();

            File.WriteAllText(tempFileName, inputCraftText);

            // when
            var returnCode = Program.Main("remove-parts", "--part", "ladder1.*", "-i", tempFileName, "--silent");

            // then
            Assert.That(returnCode, Is.EqualTo(0));
            Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
        }

        [Test]
        public void CanRemoveEnginesFromAllBowserCrafts()
        {
            // given
            const String tempFileName = "temp.txt";

            var inputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Refuel at Minmus.in.sfs")).ReadToEnd();
            var expectedOutputCraftText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Refuel at Minmus.expected.sfs")).ReadToEnd();

            File.WriteAllText(tempFileName, inputCraftText);

            // when
            var returnCode = Program.Main("remove-parts", "--part", "liquidEngine.*", "--craft", "Bowser.*", "-i", tempFileName, "--silent");

            // then
            Assert.That(returnCode, Is.EqualTo(0));
            Assert.That(File.ReadAllText(tempFileName), Is.EqualTo(expectedOutputCraftText));
        }
    }
}
